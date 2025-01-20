using System.Text;
using servers_api.Factory.Abstractions;
using ILogger = Serilog.ILogger;

namespace servers_api.Factory.TCP;

public class TcpClient : IClient
{
	private readonly ILogger _logger;

	// Конструктор с инъекцией логгера
	public TcpClient(ILogger logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	// Метод для подключения к серверу с логированием
	public async Task ConnectToServerAsync(string host, int port)
	{
		try
		{
			_logger.Information($"Попытка подключения к серверу {host}:{port}...");

			using var client = new System.Net.Sockets.TcpClient();
			await client.ConnectAsync(host, port);

			var result = client.Connected;
			_logger.Information($"Статус соединения: {result}");

			using var stream = client.GetStream();
			var message = "ping";
			var buffer = Encoding.UTF8.GetBytes(message);

			// Отправка сообщения "ping"
			await stream.WriteAsync(buffer, 0, buffer.Length);
			_logger.Information($"Отправлено сообщение: {message}");

			// Чтение ответа от сервера
			buffer = new byte[256];
			int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
			var response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
			_logger.Information($"Получен ответ от сервера: {response}");
		}
		catch (Exception ex)
		{
			// Логирование ошибки
			_logger.Error($"Ошибка при подключении: {ex.Message}");
		}
	}
}