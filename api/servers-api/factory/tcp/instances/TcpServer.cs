using System.Net.Sockets;
using System.Net;
using servers_api.factory.abstractions;
using servers_api.factory.tcp.instancehandlers;
using servers_api.models.responce;
using servers_api.models.internallayerusage.instance;

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
	ServerInstanceModel instanceModel,
	CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(instanceModel.Host))
				return new ResponceIntegration { Message = "Host cannot be null or empty.", Result = false };

			if (instanceModel.Port == 0)
			{
				_logger.LogError("Port is not specified. Unable to start the server.");
				return new ResponceIntegration { Message = "Port is not specified.", Result = false };
			}

			if (!IPAddress.TryParse(instanceModel.Host, out var ipAddress))
			{
				_logger.LogError("Invalid host address: {Host}", instanceModel.Host);
				return new ResponceIntegration { Message = "Invalid host address.", Result = false };
			}

			var listener = new TcpListener(ipAddress, instanceModel.Port);
			try
			{
				listener.Start();
				_logger.LogInformation("TCP server started on {Host}:{Port}", instanceModel.Host, instanceModel.Port);

				// Используем настройки из instanceModel.ServerConnectionSettings
				var serverSettings = instanceModel.ServerConnectionSettings;

				// Пробуем несколько раз подключиться
				for (int attempt = 1; attempt <= serverSettings.AttemptsToFindBus; attempt++)
				{
					try
					{
						var serverStartedTask = Task.Run(async () =>
						{
							await _tcpServerHandler.WaitForClientAsync(listener, serverSettings.BusResponseWaitTimeMs, cancellationToken);
						});

						// Устанавливаем таймаут для ожидания клиента
						var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(serverSettings.BusIdleTimeoutMs), cancellationToken);

						// Ожидаем либо подключения, либо таймаута
						var completedTask = await Task.WhenAny(serverStartedTask, timeoutTask);

						if (completedTask == timeoutTask)
						{
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
						_logger.LogError(ex, "Error during connection attempt {Attempt}", attempt);
						await Task.Delay(serverSettings.BusReconnectDelayMs); // Задержка перед следующей попыткой
					}
				}

				return new ResponceIntegration { Message = "Failed to connect after multiple attempts.", Result = false };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Critical error occurred while running the server.");
				return new ResponceIntegration { Message = "Critical server error.", Result = false };
			}
		}
	}
}