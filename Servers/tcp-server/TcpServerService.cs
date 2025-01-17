using System.Net.Sockets;
using System.Text;

namespace tcp_server
{
	/// <summary>
	/// Это сервер, у него есть лисенер.
	/// Он ждет, когда к нему подключится клиент.
	/// </summary>
	public class TcpServerService : BackgroundService
	{
		private readonly ILogger<TcpServerService> _logger;
		private readonly TcpListener _listener;

		public TcpServerService(ILogger<TcpServerService> logger, int port)
		{
			_logger = logger;
			_listener = new TcpListener(System.Net.IPAddress.Any, port);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_listener.Start();
			_logger.LogInformation("TCP-сервер0 ожидает подключения на порту {Port}...", _listener.LocalEndpoint);

			while (!stoppingToken.IsCancellationRequested)
			{
				using var client = await _listener.AcceptTcpClientAsync();
				_logger.LogInformation("Подключен клиент {Client}", client.Client.RemoteEndPoint);

				using var stream = client.GetStream();
				var buffer = new byte[256];
				int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
				var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

				_logger.LogInformation($"TCP-сервер0: Получено сообщение: {message}");
			}
		}
	}
}
