using servers_api.factory.abstractions;
using servers_api.models.internallayer.instance;
using servers_api.models.responces;
using System.Text;

public class TcpClient : IUpClient
{
	private readonly ILogger<TcpClient> _logger;
	private System.Net.Sockets.TcpClient _client;

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
		int maxAttempts = instanceModel.ClientConnectionSettings.AttemptsToFindExternalServer;
		int timeout = instanceModel.ClientConnectionSettings.ConnectionTimeoutMs;

		for (int attempt = 1; attempt <= maxAttempts; attempt++)
		{
			_logger.LogInformation($"Попытка {attempt} из {maxAttempts} подключения к {serverHost}:{serverPort}...");

			_client = new System.Net.Sockets.TcpClient();

			try
			{
				await _client.ConnectAsync(serverHost, serverPort);
				if (_client.Connected)
				{
					_logger.LogInformation($"Подключение к {serverHost}:{serverPort} установлено на попытке {attempt}.");

					await SendWelcomeMessageAsync();

					_ = Task.Run(() => ReceiveMessagesAsync(token), token);

					return new ResponceIntegration { Message = "Успешное подключение", Result = true };
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Ошибка подключения: {ex.Message}");
			}

			if (attempt < maxAttempts)
			{
				_logger.LogWarning($"Ожидание {timeout} мс перед следующей попыткой...");
				await Task.Delay(timeout, token);
			}
		}

		_logger.LogError($"Не удалось подключиться к {serverHost}:{serverPort} за {maxAttempts} попыток.");
		return new ResponceIntegration { Message = $"Не удалось подключиться", Result = false };
	}

	private async Task SendWelcomeMessageAsync()
	{
		try
		{
			if (_client?.Connected == true)
			{
				var stream = _client.GetStream();
				byte[] data = Encoding.UTF8.GetBytes("привет от tcp клиента");
				await stream.WriteAsync(data, 0, data.Length);
				_logger.LogInformation("Отправлено приветственное сообщение серверу.");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"Ошибка при отправке приветственного сообщения: {ex.Message}");
		}
	}

	private async Task ReceiveMessagesAsync(CancellationToken token)
	{
		if (_client?.Connected != true) return;

		var stream = _client.GetStream();
		var buffer = new byte[1024];

		_logger.LogInformation("Ожидание сообщений от сервера...");

		while (!token.IsCancellationRequested)
		{
			try
			{
				int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
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
				_logger.LogError($"Ошибка при чтении данных: {ex.Message}");
				break;
			}
		}

		_client.Close();
		_logger.LogWarning("Клиентское соединение закрыто.");
	}


}
