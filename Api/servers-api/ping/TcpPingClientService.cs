using System.Net.Sockets;
using System.Text;
using Serilog;

namespace servers_api.ping
{
	public class TcpPingClientService : ITcpPingClientService
	{
		public async Task<string> PingServerAsync(string host1, int? port1)
		{
			//string host = "127.0.0.1"; // IP-адрес сервера
			//int port = 5017;          // Порт сервера
			//string response = "";
			try
			{
			//	using var client = new TcpClient();
			//	Console.WriteLine($"Подключение к серверу {host}:{port}...");
			//	await client.ConnectAsync(host, port);
			//	var result = client.Connected;

			//	Console.WriteLine(result);
			//	Console.WriteLine();

			//	using var stream = client.GetStream();
			//	var message = "ping";
			//	var buffer = Encoding.UTF8.GetBytes(message);

			//	// Отправляем "ping"
			//	await stream.WriteAsync(buffer, 0, buffer.Length);
			//	Console.WriteLine($"Отправлено сообщение: {message}");

			//	// Читаем ответ от сервера
			//	buffer = new byte[256];
			//	int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
			//	response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
			//	Console.WriteLine($"Получен ответ от сервера: {response}");
			//	return response;
				return default;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка: {ex.Message}");
			}

			return default;
		}
	}
}
