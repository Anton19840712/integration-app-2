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

	// Метод для подключения к серверу с логированием и повторными попытками
	public async Task ConnectToServerAsync(string host, int port)
	{
		var maxAttempts = 12;  // Максимальное количество попыток (1 минута / 5 секунд = 12 попыток)
		var attempt = 0;

		while (attempt < maxAttempts)
		{
			try
			{
				_logger.Information($"Попытка подключения к серверу {host}:{port}, попытка {attempt + 1} из {maxAttempts}...");

				using var client = new System.Net.Sockets.TcpClient();
				await client.ConnectAsync(host, port);

				var result = client.Connected;
				_logger.Information($"Статус соединения: {result}");

				if (result)
				{
					_logger.Information($"Успешно подключено к серверу {host}:{port}");
					return;  // Успешное подключение, выходим из метода
				}
				else
				{
					_logger.Warning($"Не удалось подключиться к серверу {host}:{port}");
				}
			}
			catch (Exception ex)
			{
				// Логирование ошибки при попытке подключения
				_logger.Error($"Ошибка при подключении: {ex.Message}");
			}

			attempt++;

			if (attempt < maxAttempts)
			{
				// Задержка 5 секунд между попытками
				await Task.Delay(5000);
			}
		}

		_logger.Error($"Не удалось подключиться к серверу {host}:{port} за {maxAttempts} попыток.");
	}
}
