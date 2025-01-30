using System.Net.Sockets;
using System.Net;
using servers_api.factory.abstractions;
using servers_api.factory.tcp.instancehandlers;
using servers_api.models.responce;

namespace servers_api.factory.tcp.instances
{
	/// <summary>
	/// Tcp server, отвечает за создание tcp server.
	/// </summary>
	public class TcpServer : IUpServer
	{
		private readonly ILogger<TcpServer> _logger;
		private readonly ITcpServerHandler _tcpServerHandler;

		public TcpServer(ILogger<TcpServer> logger, ITcpServerHandler tcpServerHandler)
		{
			_logger = logger;
			_tcpServerHandler = tcpServerHandler;
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
				var serverStartedTask = Task.Run(async () => await _tcpServerHandler.WaitForClientAsync(listener, cancellationToken));

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
	}
}
