using System.Net.Sockets;
using System.Net;
using System.Text;
using servers_api.factory.abstractions;
using servers_api.models;

namespace servers_api.Factory.TCP
{
	public class TcpServer : IUpServer
	{
		private readonly ILogger<TcpServer> _logger;

		public TcpServer(ILogger<TcpServer> logger)
		{
			_logger = logger;
		}

		public async Task<ResponceIntegration> UpServerAsync(
			string host,
			int? port,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(host))
				return new ResponceIntegration { Message = "Host cannot be null or empty.", Result = false };

			if (!port.HasValue)
			{
				_logger.LogError("Port is not specified. Unable to start the server.");
				return new ResponceIntegration { Message = "Port is not specified.", Result = false };
			}

			if (!IPAddress.TryParse(host, out var ipAddress))
			{
				_logger.LogError("Invalid host address: {Host}", host);
				return new ResponceIntegration { Message = "Invalid host address.", Result = false };
			}

			var listener = new TcpListener(ipAddress, port.Value);

			try
			{
				listener.Start();
				_logger.LogInformation("TCP server started on {Host}:{Port}", host, port);

				// Возвращаем успешный ответ о запуске сервера
				var serverStartedTask = Task.Run(async () => await WaitForClientAsync(listener, cancellationToken));

				// Устанавливаем таймаут на ожидание
				var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

				// Ожидаем либо подключения, либо таймаута
				var completedTask = await Task.WhenAny(serverStartedTask, timeoutTask);

				if (completedTask == timeoutTask)
				{
					// Таймаут: сервер успешно запустился, но клиент не подключился в течение времени
					_logger.LogInformation("No client connected within the timeout period.");
					return new ResponceIntegration
					{
						Message = "Server started, but no client connected within the timeout period.",
						Result = true
					};
				}

				// Если клиент подключился
				return new ResponceIntegration
				{
					Message = "Server started and waiting for client connections.",
					Result = true
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Critical error occurred while running the server.");
				return new ResponceIntegration { Message = "Critical server error.", Result = false };
			}
		}

		private async Task WaitForClientAsync(TcpListener listener, CancellationToken cancellationToken)
		{
			try
			{
				// Ожидаем подключения клиента
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						var client = await listener.AcceptTcpClientAsync(cancellationToken);
						_logger.LogInformation("Client connected: {Client}", client.Client.RemoteEndPoint);

						// Обрабатываем клиента в отдельной задаче
						_ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
					}
					catch (OperationCanceledException)
					{
						_logger.LogInformation("Server shutdown is requested.");
						break;
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error accepting client connection.");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during client connection waiting.");
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
					_logger.LogWarning("Client {Client} disconnected.", client.Client.RemoteEndPoint);
					return;
				}

				var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				_logger.LogInformation("Received message from {Client}: {Message}", client.Client.RemoteEndPoint, message);

				// Отправляем ответ
				var response = Encoding.UTF8.GetBytes("Message received.");
				await stream.WriteAsync(response.AsMemory(0, response.Length), cancellationToken);
				_logger.LogInformation("Response sent to {Client}.", client.Client.RemoteEndPoint);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error handling client {Client}.", client.Client.RemoteEndPoint);
			}
			finally
			{
				client.Close();
				_logger.LogInformation("Connection with client {Client} closed.", client.Client.RemoteEndPoint);
			}
		}
	}
}
