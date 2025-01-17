using Serilog;
using tcp_server;

public class Program
{
	public static async Task Main(string[] args)
	{
		Console.Title = "server-0, 5000";

		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateLogger();

		int port = 5000; // Порт для TcpServer0

		var host = Host.CreateDefaultBuilder(args)
			.UseSerilog()
			.ConfigureServices(services =>
			{
				services.AddSingleton(provider => new TcpServerService(provider.GetRequiredService<ILogger<TcpServerService>>(), port));
				services.AddHostedService(provider => provider.GetRequiredService<TcpServerService>());
			})
			.Build();

		// Получение адреса сервера и логирование
		var serverService = host.Services.GetRequiredService<TcpServerService>();
		Log.Information("Сервер доступен по адресу: {Address}", serverService.GetServerAddress());

		await host.RunAsync();
	}
}

