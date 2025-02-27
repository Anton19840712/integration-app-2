using servers_api.enums;
using servers_api.factory.abstractions;
using servers_api.models.internallayer.instance;
using servers_api.models.outbox;
using servers_api.models.response;
using servers_api.repositories;
using System.Net.Sockets;
using System.Text;

public class TcpClientInstance : IUpClient
{
	private readonly ILogger<TcpClientInstance> _logger;
	private readonly IMongoRepository<OutboxMessage> _outboxRepository;
	private string _serverHost;
	private int _serverPort;
	private TcpClient _tcpClient;
	private NetworkStream _networkStream;
	private int _reconnectAttempts;
	private const int MaxReconnectAttempts = 5;
	private List<string> _messageQueue = new List<string>(); // Промежуточная коллекция для сообщений

	public TcpClientInstance(ILogger<TcpClientInstance> logger, IMongoRepository<OutboxMessage> outboxRepository)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
		_serverHost = "127.0.0.1";
		_serverPort = 6254;
		_reconnectAttempts = 0;
	}

	public async Task<ResponseIntegration> ConnectToServerAsync(
		ClientInstanceModel instanceModel,
		string serverHost,
		int serverPort,
		CancellationToken token)
	{
		_serverHost = string.IsNullOrWhiteSpace(serverHost) ? "127.0.0.1" : serverHost;
		_serverPort = serverPort == 0 ? 6254 : serverPort;

		while (_reconnectAttempts < MaxReconnectAttempts)
		{
			try
			{
				_tcpClient = new TcpClient();
				_logger.LogInformation("Подключение к серверу {ServerHost}:{ServerPort}...", _serverHost, _serverPort);
				await _tcpClient.ConnectAsync(_serverHost, _serverPort, token);
				_networkStream = _tcpClient.GetStream();

				_logger.LogInformation("Подключение успешно установлено.");

				// Запускаем фоновое чтение сообщений
				_ = Task.Run(() => ReadMessagesAsync(instanceModel.InQueueName, instanceModel.OutQueueName, token), token);

				return new ResponseIntegration { Message = "Успешное подключение", Result = true };
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
			{
				_logger.LogWarning("Соединение с сервером было разорвано. Попытка повторного подключения...");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при подключении к серверу.");
			}

			_reconnectAttempts++;
			if (_reconnectAttempts < MaxReconnectAttempts)
			{
				var delayTime = 5000 * _reconnectAttempts;
				_logger.LogWarning("Попытка подключения {Attempt} из {MaxAttempts}. Повторное подключение через {DelayTime} миллисекунд.",
					_reconnectAttempts, MaxReconnectAttempts, delayTime);
				await Task.Delay(delayTime, token);
			}
			else
			{
				_logger.LogError("Превышено максимальное количество попыток подключения.");
				return new ResponseIntegration { Message = "Ошибка подключения", Result = false };
			}
		}

		return new ResponseIntegration { Message = "Ошибка подключения", Result = false };
	}

	private async Task ReadMessagesAsync(
		string instanceModelQueueInName,
		string instanceModelQueueOutName,
		CancellationToken token)
	{
		if (_networkStream == null) return;

		var buffer = new byte[256];

		while (!token.IsCancellationRequested)
		{
			try
			{
				if (!_tcpClient.Connected)
				{
					_logger.LogWarning("Соединение разорвано. Попытка переподключения...");
					await ConnectToServerAsync(null, _serverHost, _serverPort, token);
					continue;
				}

				int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, token);

				if (bytesRead == 0)
				{
					_logger.LogWarning("Соединение с сервером закрыто.");
					break;
				}

				var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
				_logger.LogInformation("Обработка полученного сообщения: {Message}", message);

				// Добавляем сообщение в промежуточную коллекцию
				lock (_messageQueue)
				{
					_messageQueue.Add(message);
				}

				// Обрабатываем и логируем сообщения
				ProcessMessages(instanceModelQueueInName, instanceModelQueueOutName);

				Array.Clear(buffer, 0, buffer.Length);
			}
			catch (Exception ex)
			{
				_logger.LogError("Ошибка при чтении данных от сервера.");
				_logger.LogError(ex.Message);
				break;
			}
		}

		_tcpClient?.Close();
		_logger.LogInformation("Соединение завершено.");
	}

	private async void ProcessMessages(string instanceModelQueueInName, string instanceModelQueueOutName)
	{
		List<string> messagesToProcess;

		// Извлекаем и обрабатываем сообщения из промежуточной коллекции
		lock (_messageQueue)
		{
			messagesToProcess = new List<string>(_messageQueue);
			_messageQueue.Clear();
		}

		foreach (var message in messagesToProcess)
		{
			try
			{
				_logger.LogInformation("Логирование сообщения: {Message}", message);

				// Сохранение полученного сообщения в базу данных
				var outboxMessage = new OutboxMessage
				{
					Id = Guid.NewGuid().ToString(),
					ModelType = "incident",
					EventType = EventTypes.Received,
					IsProcessed = false,
					ProcessedAt = DateTime.Now,
					InQueue = instanceModelQueueInName,
					OutQueue = instanceModelQueueOutName,
					Payload = message,
					RoutingKey = "routing_key_tcp",
					CreatedAt = DateTime.UtcNow,
					CreatedAtFormatted = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
					Source = $"tcp-client-instance hosted on host: {_serverHost} and port {_serverPort}"
				};

				await _outboxRepository.SaveMessageAsync(outboxMessage); // Сохранение в MongoDB
			}
			catch (Exception ex)
			{
				_logger.LogError("Ошибка при сохранении сообщения в базу данных.");
				_logger.LogError(ex.Message);
			}
		}
	}
}
