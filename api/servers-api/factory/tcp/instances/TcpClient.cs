using servers_api.factory.abstractions;
using servers_api.models.internallayer.instance;
using servers_api.models.responces;
using System.Net.Sockets;
using System.Text;

public class TcpClient : IUpClient
{
	private readonly ILogger<TcpClient> _logger;
	private System.Net.Sockets.TcpClient _client;
	private NetworkStream _stream;
	private string _serverHost;
	private int _serverPort;
	private CancellationTokenSource _cts;

	public TcpClient(ILogger<TcpClient> logger)
	{
		_logger = logger;
		_logger.LogInformation("TcpClient instance created.");
	}

	public async Task<ResponceIntegration> ConnectToServerAsync(
		ClientInstanceModel instanceModel,
		string serverHost,
		int serverPort,
		CancellationToken token)
	{
		_serverHost = serverHost;
		_serverPort = serverPort;
		_cts = CancellationTokenSource.CreateLinkedTokenSource(token);

		int maxAttempts = instanceModel.ClientConnectionSettings.AttemptsToFindExternalServer;
		int timeout = instanceModel.ClientConnectionSettings.ConnectionTimeoutMs;

		for (int attempt = 1; attempt <= maxAttempts; attempt++)
		{
			_logger.LogInformation($"Попытка {attempt} из {maxAttempts} подключения к {serverHost}:{serverPort}...");

			if (await TryConnectAsync(instanceModel))
			{
				_logger.LogInformation($"Подключение к {serverHost}:{serverPort} установлено на попытке {attempt}.");
				_ = Task.Run(MonitorConnectionAsync, _cts.Token);
				return new ResponceIntegration { Message = "Успешное подключение", Result = true };
			}

			_logger.LogWarning($"Ожидание {timeout} мс перед следующей попыткой...");
			await Task.Delay(timeout, token);
		}

		_logger.LogError($"Не удалось подключиться к {serverHost}:{serverPort} за {maxAttempts} попыток.");
		return new ResponceIntegration { Message = "Не удалось подключиться", Result = false };
	}

	private async Task<bool> TryConnectAsync(ClientInstanceModel instanceModel = null)
	{
		try
		{
			_client?.Close();
			_client = new System.Net.Sockets.TcpClient();

			BindLocalEndpoint(instanceModel);
			await _client.ConnectAsync(_serverHost, _serverPort);

			if (_client.Connected)
			{
				_stream = _client.GetStream();
				await SendWelcomeMessageAsync();
				_ = Task.Run(() => ReceiveMessagesAsync(_cts.Token), _cts.Token);
				return true;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка подключения к серверу.");
		}
		return false;
	}

	private void BindLocalEndpoint(ClientInstanceModel instanceModel)
	{
		if (!string.IsNullOrEmpty(instanceModel?.ClientHost) && instanceModel.ClientPort > 0)
		{
			var localEndPoint = new System.Net.IPEndPoint(
				System.Net.IPAddress.Parse(instanceModel.ClientHost),
				instanceModel.ClientPort);

			_logger.LogInformation($"Привязываем клиента к локальному адресу {localEndPoint}");
			_client.Client.Bind(localEndPoint);
			_logger.LogInformation($"Фактический локальный адрес: {_client.Client.LocalEndPoint}");
		}
	}

	private async Task SendWelcomeMessageAsync()
	{
		try
		{
			if (_client?.Connected == true)
			{
				byte[] data = Encoding.UTF8.GetBytes("привет от tcp клиента");
				await _stream.WriteAsync(data, 0, data.Length);
				_logger.LogInformation("Отправлено приветственное сообщение серверу.");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при отправке приветственного сообщения.");
		}
	}

	private async Task ReceiveMessagesAsync(CancellationToken token)
	{
		var buffer = new byte[1024];
		_logger.LogInformation("Ожидание сообщений от сервера...");

		while (!token.IsCancellationRequested && _client.Connected)
		{
			try
			{
				int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
				if (bytesRead == 0)
				{
					_logger.LogWarning("Соединение закрыто сервером.");
					break;
				}

				string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				_logger.LogInformation($"Получено сообщение от сервера: {message}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при чтении данных.");
				break;
			}
		}

		_logger.LogWarning("Клиентское соединение закрыто.");
		await ReconnectAsync();
	}

	private async Task MonitorConnectionAsync()
	{
		while (!_cts.Token.IsCancellationRequested)
		{
			await Task.Delay(5000, _cts.Token);
			if (!_client.Connected)
			{
				_logger.LogWarning("Соединение потеряно. Попытка переподключения...");
				await ReconnectAsync();
			}
		}
	}

	private async Task ReconnectAsync()
	{
		_logger.LogInformation("Переподключение к серверу...");
		while (!_cts.Token.IsCancellationRequested)
		{
			if (await TryConnectAsync())
			{
				_logger.LogInformation("Успешное восстановление соединения.");
				return;
			}
			await Task.Delay(5000, _cts.Token);
		}
	}

	public void Disconnect()
	{
		_logger.LogInformation("Отключение клиента...");
		_cts?.Cancel();
		_client?.Close();
	}
}
