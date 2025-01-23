using System.Net.Sockets;
using System.Net;
using System.Text;
using servers_api.Factory.Abstractions;
using ILogger = Serilog.ILogger;

namespace servers_api.Factory.TCP
{
	public class TcpServer : IServer
	{
		private readonly ILogger _logger;

		public TcpServer(ILogger logger)
		{
			_logger = logger;
		}

		public async Task UpServerAsync(string host, int? port, CancellationToken cancellationToken = default)
		{
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

			try
			{
				var listener = new TcpListener(ipAddress, port.Value);
				listener.Start();

				_logger.Information("Server is running on {Host}:{Port}", host, port.Value);
				_logger.Information("Waiting for a connection...");

				while (!cancellationToken.IsCancellationRequested)
				{
					var client = await listener.AcceptTcpClientAsync(cancellationToken);
					_ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
				}

				listener.Stop();
				_logger.Information("Server has stopped.");
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "An error occurred while running the TCP server.");
			}
		}

		private async Task HandleClientAsync(System.Net.Sockets.TcpClient client, CancellationToken cancellationToken)
		{
			try
			{
				_logger.Information("Connection accepted from {Client}", client.Client.RemoteEndPoint);

				using var stream = client.GetStream();
				var buffer = new byte[256];
				int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
				var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

				_logger.Information("Received message: {Message}", message);

				var response = "The message was received by the server.";
				var responseBytes = Encoding.UTF8.GetBytes(response);
				await stream.WriteAsync(responseBytes, cancellationToken);

				_logger.Information("Sent acknowledgement to client.");
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "An error occurred while handling a client connection.");
			}
			finally
			{
				client.Close();
				_logger.Information("Client connection closed.");
			}
		}
	}
}
