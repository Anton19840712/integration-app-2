using System.Net.Sockets;
using System.Text;
using servers_api.factory.abstractions;
using servers_api.models.internallayer.instance;
using servers_api.models.responces;

namespace servers_api.factory.tcp.instances
{
	/// <summary>
	/// Класс, отвечающий за создание tcp client instance.
	/// </summary>
	public class TcpClient : IUpClient
	{
		private readonly ILogger<TcpClient> _logger;

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
				_logger.LogInformation($"Начинается попытка {attempt} из {maxAttempts} подключения к {serverHost}:{serverPort}...");

				using var client = new System.Net.Sockets.TcpClient();

				// Привязка клиента к локальному адресу и порту
				if (!string.IsNullOrEmpty(instanceModel.ClientHost) && instanceModel.ClientPort > 0)
				{
					var localEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(instanceModel.ClientHost), instanceModel.ClientPort);

					_logger.LogInformation($"Привязываем клиента к локальному адресу {instanceModel.ClientHost}:{instanceModel.ClientPort}");
					client.Client.Bind(localEndPoint);
					var actualEndPoint = (System.Net.IPEndPoint)client.Client.LocalEndPoint;
					_logger.LogInformation($"Клиент привязан к локальному адресу {actualEndPoint.Address}:{actualEndPoint.Port}");
				}

				var connectTask = client.ConnectAsync(serverHost, serverPort);
				var delayTask = Task.Delay(timeout);

				var completedTask = await Task.WhenAny(connectTask, delayTask);

				if (completedTask == connectTask && client.Connected)
				{
					_logger.LogInformation($"Успешно подключено к {serverHost}:{serverPort} на попытке {attempt}.");

					// Отправляем приветственное сообщение серверу
					await SendWelcomeMessageAsync(client);

					// Начинаем слушать сообщения от сервера
					_logger.LogInformation("Запуск фонового процесса чтения сообщений...");
					_ = Task.Run(() => ReceiveMessagesAsync(client, token), token);

					return new ResponceIntegration { Message = "Успешное подключение", Result = true };
				}

				_logger.LogWarning($"Попытка {attempt} из {maxAttempts} не завершена в срок (тайм-аут). Продолжаем попытки...");

				if (attempt < maxAttempts)
				{
					_logger.LogWarning($"Ожидание {timeout} мс перед следующей попыткой {attempt + 1}...");
					try
					{
						await Task.Delay(timeout, CancellationToken.None); // Принудительное ожидание без отмены
					}
					catch (TaskCanceledException) { }
				}
			}

			_logger.LogInformation($"Не удалось подключиться к {serverHost}:{serverPort} за {maxAttempts} попыток.");
			return new ResponceIntegration { Message = $"Не удалось подключиться после {maxAttempts} попыток", Result = false };
		}

		private async Task SendWelcomeMessageAsync(System.Net.Sockets.TcpClient client)
		{
			try
			{
				var stream = client.GetStream();
				string welcomeMessage = "привет от tcp клиента безопасного города";
				byte[] data = Encoding.UTF8.GetBytes(welcomeMessage);
				await stream.WriteAsync(data, 0, data.Length);
				_logger.LogInformation("Отправлено приветственное сообщение серверу.");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Ошибка при отправке приветственного сообщения: {ex.Message}");
			}
		}

		private async Task ReceiveMessagesAsync(System.Net.Sockets.TcpClient client, CancellationToken token)
		{
			var stream = client.GetStream();
			var buffer = new byte[1024];

			_logger.LogInformation("Ожидание сообщений от сервера...");

			while (!token.IsCancellationRequested)
			{
				try
				{
					int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
					if (bytesRead == 0)
					{
						_logger.LogWarning("Соединение с сервером закрыто.");
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
		}
	}
}
