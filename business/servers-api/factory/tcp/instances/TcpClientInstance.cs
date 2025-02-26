using System.Net.Sockets;
using System.Text;
using servers_api.factory.abstractions;
using servers_api.models.internallayer.instance;
using servers_api.models.response;
using servers_api.models.outbox;
using servers_api.repositories;
using servers_api.enums;

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
				_ = Task.Run(() => ReadMessagesAsync(token), token);

				return new ResponseIntegration { Message = "Успешное подключение", Result = true };
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
			{
				// Сервер принудительно разорвал соединение
				_logger.LogWarning("Соединение с сервером было разорвано. Попытка повторного подключения...");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при подключении к серверу.");
			}

			_reconnectAttempts++;
			if (_reconnectAttempts < MaxReconnectAttempts)
			{
				var delayTime = 5000 * _reconnectAttempts; // Увеличиваем время ожидания с каждой попыткой
				_logger.LogWarning("Попытка подключения {Attempt} из {MaxAttempts}. Повторное подключение через {DelayTime} миллисекунд.",
					_reconnectAttempts, MaxReconnectAttempts, delayTime);
				await Task.Delay(delayTime, token); // Задержка перед повторным подключением
			}
			else
			{
				_logger.LogError("Превышено максимальное количество попыток подключения.");
				return new ResponseIntegration { Message = "Ошибка подключения", Result = false };
			}
		}

		return new ResponseIntegration { Message = "Ошибка подключения", Result = false };
	}

	private async Task ReadMessagesAsync(CancellationToken token)
	{
		if (_networkStream == null) return;

		var buffer = new byte[256];

		while (!token.IsCancellationRequested)
		{
			try
			{
				if (!_tcpClient.Connected) // Проверка состояния соединения
				{
					_logger.LogWarning("Соединение разорвано. Попытка переподключения...");
					await ConnectToServerAsync(null, _serverHost, _serverPort, token); // Попытка переподключения
					continue; // Возврат в начало цикла для повторной попытки
				}

				int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, token);

				if (bytesRead == 0)
				{
					_logger.LogWarning("Соединение с сервером закрыто.");
					break;
				}

				var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
				_logger.LogInformation("Обработка полученного сообщения: {Message}", message);

				// Сохранение полученного сообщения в базу данных
				var outboxMessage = new OutboxMessage
				{
					Id = Guid.NewGuid().ToString(),
					ModelType = "incident",
					EventType = EventTypes.Received,
					IsProcessed = false,
					ProcessedAt = DateTime.Now,
					OutQueue = "outbox_queue",
					InQueue = "inbox_queue",
					Payload = message,
					RoutingKey = "message_routing_key", // Пример
					CreatedAt = DateTime.UtcNow,
					CreatedAtFormatted = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
					Source = "tcp-client-instance"
				};

				await _outboxRepository.SaveMessageAsync(outboxMessage); // Сохранение сообщения в MongoDB

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
}
