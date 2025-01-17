using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TcpClientService : BackgroundService
{
    private readonly ILogger<TcpClientService> _logger;
    private TcpListener _listener;
    private readonly List<TcpClient> _activeClients = new();

    public TcpClientService(ILogger<TcpClientService> logger)
    {
        _logger = logger;
        _listener = new TcpListener(IPAddress.Any, 6001);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener.Start();
        _logger.LogInformation("TcpClient ожидает подключения...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                _activeClients.Add(client);
                _ = HandleClientAsync(client, stoppingToken); // Обработка каждого клиента в отдельном потоке
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подключении клиента.");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using var networkStream = client.GetStream();

        while (client.Connected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var buffer = new byte[256];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead == 0) break; // Закрытие соединения

                var serverAddress = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _logger.LogInformation("Получен адрес: {Address}", serverAddress);

                var (ip, port) = ParseAddress(serverAddress);

                if (await TryConnectToServerAsync(ip, port, cancellationToken))
                {
                    _logger.LogInformation("Подключение к {Address} успешно", serverAddress);
                }
                else
                {
                    var errorMessage = Encoding.UTF8.GetBytes("Ошибка подключения");
                    await networkStream.WriteAsync(errorMessage, 0, errorMessage.Length, cancellationToken);
                    _logger.LogWarning("Подключение к {Address} не удалось", serverAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке клиента.");
                break;
            }
        }

        _activeClients.Remove(client);
        client.Close();
    }

    private async Task<bool> TryConnectToServerAsync(string ip, int port, CancellationToken cancellationToken)
    {
        try
        {
            using var serverClient = new TcpClient();
            await serverClient.ConnectAsync(ip, port);
            _logger.LogInformation("Подключено к серверу {Ip}:{Port}", ip, port);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось подключиться к серверу {Ip}:{Port}", ip, port);
            return false;
        }
    }

    private (string, int) ParseAddress(string address)
    {
        var parts = address.Split(':');
        return (parts[0], int.Parse(parts[1]));
    }
}
