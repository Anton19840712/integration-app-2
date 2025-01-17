using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using tcp_server_2;

public class Program
{
    public static async Task Main(string[] args)
    {

        Console.Title = "server-2, 5002";

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        int port = 5002; // Порт для TcpServer2

        await Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService(provider => new TcpServerService(provider.GetRequiredService<ILogger<TcpServerService>>(), port));
            })
            .RunConsoleAsync();
    }
}
