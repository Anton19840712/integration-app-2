using System.Net.Sockets;
using System.Text;

namespace servers_api.background
{
	public class TcpPingBackgroundService : BackgroundService
	{
		private readonly string _host = "127.0.0.1"; // IP-адрес сервера
		private readonly int _port = 5018;          // Порт сервера

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var client = new TcpClient();
					Console.WriteLine($"Подключение к серверу {_host}:{_port}...");
					await client.ConnectAsync(_host, _port);
					if (client.Connected)
					{
						Console.WriteLine("Подключение успешно.");
					}

					using var stream = client.GetStream();
					var message = "ping";
					var buffer = Encoding.UTF8.GetBytes(message);

					// Отправляем "ping"
					await stream.WriteAsync(buffer, 0, buffer.Length, stoppingToken);
					Console.WriteLine($"Отправлено сообщение: {message}");

					// Читаем ответ от сервера
					buffer = new byte[256];
					int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
					var response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
					Console.WriteLine($"Получен ответ от сервера: {response}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка: {ex.Message}");
				}

				// Интервал между пингами
				await Task.Delay(5000, stoppingToken); // Пинг каждые 5 секунд
			}
		}
	}
}