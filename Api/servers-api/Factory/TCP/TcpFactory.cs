using servers_api.factory.abstractions;
using servers_api.Factory.TCP;

public class TcpFactory : ProtocolFactory
{
	private readonly ILogger<TcpFactory> _logger;
	private readonly ILogger<TcpServer> _serverLogger;
	private readonly ILogger<TcpClient> _clientLogger;

	public TcpFactory(
		ILogger<TcpFactory> logger,
		ILogger<TcpServer> serverLogger,
		ILogger<TcpClient> clientLogger)
	{
		_logger = logger;
		_serverLogger = serverLogger;
		_clientLogger = clientLogger;
	}

	public override IUpServer CreateServer()
	{
		_logger.LogInformation("Creating a TcpServer instance.");
		return new TcpServer(_serverLogger);
	}

	public override IUpClient CreateClient()
	{
		_logger.LogInformation("Creating a TcpClient instance.");
		return new TcpClient(_clientLogger);
	}
}
