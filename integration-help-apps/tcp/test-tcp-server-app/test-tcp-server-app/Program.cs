using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;

class Program
{
	const int Port = 5018;

	static async Task Main(string[] args)
	{
		var config = BuildConfig();

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(config)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.CreateLogger();

		string message = config["TcpSettings:Message"];

		if (string.IsNullOrWhiteSpace(message))
		{
			Console.WriteLine("❌ Message не загружен из конфигурации!");
			return;
		}

		Console.Title = "outside tcp server simulation";
		var listener = new TcpListener(IPAddress.Any, Port);

		listener.Start();

		Log.Information("Сервер запущен на порту {Port}", Port);

		while (true)
		{
			TcpClient client = await listener.AcceptTcpClientAsync();
			Log.Information("Клиент подключен");
			_ = Task.Run(() => HandleClientAsync(client, message));
		}
	}

	static IConfiguration BuildConfig()
	{
		var builder = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
			.AddEnvironmentVariables();

		return builder.Build();
	}

	static async Task HandleClientAsync(TcpClient client, string message)
	{
		try
		{
			using (client)
			using (var stream = client.GetStream())
			{
				int messageCount = 1;
				byte[] buffer = new byte[1024];

				while (client.Connected)
				{
					if (stream.DataAvailable)
					{
						int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
						if (bytesRead == 0) break;
						Log.Information("Получено сообщение: {Data}", Encoding.UTF8.GetString(buffer, 0, bytesRead));
					}

					byte[] payload = Encoding.UTF8.GetBytes(message);
					byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
					if (!BitConverter.IsLittleEndian)
						Array.Reverse(lengthPrefix); // гарантируем little-endian

					await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
					await stream.WriteAsync(payload, 0, payload.Length);

					Log.Information("Отправлено сообщение номер {Count}", messageCount);

					messageCount++;
					Log.Information("TCP server is running: {Port} - {Time}", Port, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

					await Task.Delay(3000);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Ошибка при обработке клиента");
		}
		finally
		{
			Log.Information("Клиент отключился");
		}
	}
}
