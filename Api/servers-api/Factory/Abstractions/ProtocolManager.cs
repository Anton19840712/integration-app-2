using servers_api.models;

namespace servers_api.factory.abstractions
{
	/// <summary>
	/// Класс, который поднимает в динамическом шлюзе
	/// согласно входящей информации либо клиент, либо сервер определенного соединения.
	/// </summary>
	public class ProtocolManager
	{
		private readonly ILogger<ProtocolManager> _logger;
		private readonly TcpFactory _tcpFactory;

		public ProtocolManager(ILogger<ProtocolManager> logger, TcpFactory tcpFactory)
		{
			_logger = logger;
			_tcpFactory = tcpFactory;
		}

		public async Task<ResponceIntegration> ConfigureAsync(
			string protocol,
			bool isServer,
			string address = null,
			string host = null,
			int? port = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(protocol))
			{
				_logger.LogError("Protocol cannot be null or empty.");

				var result = new ResponceIntegration { Message = "Protocol is required.", Result = false };
				return result;
			}

			if (string.IsNullOrWhiteSpace(host))
			{
				_logger.LogError("Host cannot be null or empty.");

				var result = new ResponceIntegration { Message = "Host is required.", Result = false };
				return result;
			}

			if (!port.HasValue)
			{
				_logger.LogWarning("Port is not specified. Using default port 5000.");
				port = 5000; // Используем порт по умолчанию, если не указан
			}

			ProtocolFactory factory = protocol switch
			{
				"TCP" => _tcpFactory,
				_ => throw new ArgumentException($"Unsupported protocol: {protocol}")
			};

			try
			{
				if (isServer)
				{
					_logger.LogInformation("Initializing TCP server on {Host}:{Port}...", host, port);
					IUpServer server = factory.CreateServer();

					var result = await server.UpServerAsync(host, port, cancellationToken);
					return result;
				}
				else
				{
					_logger.LogInformation("Initializing TCP client to connect {Host}:{Port}...", host, port);
					IUpClient client = factory.CreateClient();

					var result = await client.ConnectToServerAsync(host, port.Value);
					return result;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during protocol configuration.");

				var result = new ResponceIntegration { Message = $"Error: {ex.Message}", Result = false };
				return result;
			}
		}
	}
}
