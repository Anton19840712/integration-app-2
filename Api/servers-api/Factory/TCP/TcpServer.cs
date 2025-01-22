using servers_api.Factory.Abstractions;
using ILogger = Serilog.ILogger;
namespace servers_api.Factory.TCP;

public class TcpServer : IServer
{
	private readonly ILogger _logger;

	public TcpServer(ILogger logger)
	{
		_logger = logger;
	}
	public void UpServer(string host, int? port)
	{
		_logger.Information("Запущен сервер");
	}
}
