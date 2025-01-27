using System.Net.Sockets;
using System.Net;
using System.Text;
using servers_api.Factory.Abstractions;
using ILogger = Serilog.ILogger;

namespace servers_api.Factory.TCP
{
	public class TcpServer : IServer
	{
		public readonly ILogger _logger;

		public TcpServer(ILogger logger)
		{
			_logger = logger;
		}

		public async Task UpServerAsync(string host, int? port, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(host))
				throw new ArgumentException("Host cannot be null or empty.", nameof(host));

			if (!port.HasValue)
			{
				_logger.Error("Port is not specified. Unable to start the server.");
				return;
			}

			if (!IPAddress.TryParse(host, out var ipAddress))
			{
				_logger.Error("Invalid host address: {Host}", host);
				return;
			}

			var listener = new TcpListener(ipAddress, port.Value);

			try
			{
				listener.Start();
				//_logger.Information("TCP server started on {Host}:{Port}", host, port);

				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						var client = await listener.AcceptTcpClientAsync(cancellationToken);
						//_logger.Information("Client connected: {Client}", client.Client.RemoteEndPoint);

						// Обрабатываем клиента в отдельной задаче
						_ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
					}
					catch (OperationCanceledException)
					{
						//_logger.Information("Server shutdown is requested.");
						break;
					}
					catch (Exception ex)
					{
						//_logger.Error(ex, "Error accepting client connection.");
					}
				}
			}
			catch (Exception ex)
			{
				//_logger.Error(ex, "Critical error occurred while running the server.");
			}
			finally
			{
				listener.Stop();
				//_logger.Information("TCP server on {Host}:{Port} has been stopped.", host, port);
			}
		}

		private async Task HandleClientAsync(System.Net.Sockets.TcpClient client, CancellationToken cancellationToken)
		{
			try
			{
				await using var stream = client.GetStream();
				var buffer = new byte[256];

				// Читаем сообщение от клиента
				int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
				if (bytesRead == 0)
				{
					//_logger.Warning("Client {Client} disconnected.", client.Client.RemoteEndPoint);
					return;
				}

				var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				//_logger.Information("Received message from {Client}: {Message}", client.Client.RemoteEndPoint, message);

				// Отправляем ответ
				var response = Encoding.UTF8.GetBytes("Message received.");
				await stream.WriteAsync(response.AsMemory(0, response.Length), cancellationToken);
				//_logger.Information("Response sent to {Client}.", client.Client.RemoteEndPoint);
			}
			catch (Exception ex)
			{
				//_logger.Error(ex, "Error handling client {Client}.", client.Client.RemoteEndPoint);
			}
			finally
			{
				client.Close();
				//_logger.Information("Connection with client {Client} closed.", client.Client.RemoteEndPoint);
			}
		}
	}
}
