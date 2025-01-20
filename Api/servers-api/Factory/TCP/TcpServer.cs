using servers_api.Factory.Abstractions;
using servers_api.start;

namespace servers_api.Factory.TCP;

public class TcpServer : IServer
    {
	public void UpServer(string host, int? port)
	{
		var runner = new TCPServerRunner();
		runner.RunTcpServer(host, port);
	}
}
