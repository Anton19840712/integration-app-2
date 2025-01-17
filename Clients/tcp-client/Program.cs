using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.Title = "tcp-client";

        // Настройка логирования
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        await Host.CreateDefaultBuilder(args)
            .UseSerilog(logger)
            .ConfigureServices(services =>
            {
                services.AddHostedService<TcpClientService>();
            })
            .RunConsoleAsync();
    }
}
