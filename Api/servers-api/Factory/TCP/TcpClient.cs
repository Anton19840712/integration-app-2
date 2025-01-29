using servers_api.factory.abstractions;
using servers_api.models;

namespace servers_api.Factory.TCP
{
	public class TcpClient : IUpClient
	{
		private readonly ILogger<TcpClient> _logger;

		public TcpClient(ILogger<TcpClient> logger)
		{
			_logger = logger;
			_logger.LogInformation("TcpClient instance created.");
		}

		// Метод для подключения к серверу с логированием и повторными попытками
		public async Task<ResponceIntegration> ConnectToServerAsync(string host, int port)
		{
			var maxAttempts = 1; // Максимальное количество попыток
			var attempt = 0;

			while (attempt < maxAttempts)
			{
				attempt++; // Увеличиваем счетчик попыток перед выполнением логики

				try
				{
					_logger.LogInformation($"Попытка подключения к серверу {host}:{port}, попытка {attempt} из {maxAttempts}...");

					using var client = new System.Net.Sockets.TcpClient();
					await client.ConnectAsync(host, port);

					if (client.Connected)
					{
						_logger.LogInformation($"Успешно подключено к серверу {host}:{port}");
						return new ResponceIntegration { Message = "Успешное подключение", Result = true };
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Ошибка при подключении: {ex.Message}");
				}

				if (attempt < maxAttempts)
				{
					_logger.LogWarning($"Ожидание перед следующей попыткой...");
					await Task.Delay(2000);
				}
			}

			_logger.LogInformation($"Не удалось подключиться к серверу {host}:{port} за {maxAttempts} попыток.");
			return new ResponceIntegration { Message = "Не удалось подключиться после нескольких попыток", Result = false };
		}
	}
}
