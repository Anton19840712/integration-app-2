using System.Text;
using servers_api.factory.abstractions;
using servers_api.models.internallayerusage.instance;
using servers_api.models.responce;

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
			var attempt = 0;

			while (attempt < instanceModel.ClientConnectionSettings.AttemptsToFindExternalServer)
			{
				attempt++;

				try
				{
					_logger.LogInformation($"Попытка подключения к серверу " +
						$"{serverHost}:{serverPort}, попытка {attempt} из {instanceModel.ClientConnectionSettings.AttemptsToFindExternalServer}...");

					var client = new System.Net.Sockets.TcpClient();
					await client.ConnectAsync(serverHost, serverPort);

					if (client.Connected)
					{
						_logger.LogInformation($"Успешно подключено к серверу {serverHost}:{serverPort}");
						_ = Task.Run(() => ReceiveMessagesAsync(client, token), token);
						return new ResponceIntegration { Message = "Успешное подключение", Result = true };
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Ошибка при подключении: {ex.Message}");
				}

				if (attempt < instanceModel.ClientConnectionSettings.AttemptsToFindExternalServer)
				{
					_logger.LogWarning($"Ожидание перед следующей попыткой...");
					await Task.Delay(instanceModel.ClientConnectionSettings.ConnectionTimeoutMs, token);
				}
			}

			_logger.LogInformation($"Не удалось подключиться к серверу {serverHost}:{serverPort} за {instanceModel.ClientConnectionSettings.AttemptsToFindExternalServer} попыток.");
			return new ResponceIntegration
			{
				Message = $"Не удалось подключиться после {attempt} попыток",
				Result = false
			};
		}

		private async Task ReceiveMessagesAsync(System.Net.Sockets.TcpClient client, CancellationToken token)
		{
			using var stream = client.GetStream();
			var buffer = new byte[1024];

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

			client.Close();
		}
	}
}



