using Serilog;
using servers_api.Factory.Abstractions;
using servers_api.ping;
using servers_api.start;

namespace servers_api.Factory.TCP;

    public class TcpServer : IServer
    {
	public void UpServer(string host, int? port)
	{
		var runner = new TCPServerRunner();
		runner.RunTcpServer(host, port);
	}

	public async Task SendServerAddress(string host, int? port)
        {
		var _tcpService = new TcpPingClientService();
	
		var result = await _tcpService.PingServerAsync(host, port);

		// Вывод результата
		Log.Information("Результат пинга: {Result}", result);
	}
}
