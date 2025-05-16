using System.Net.Sockets;
using CommonGateLib.Connections;

namespace api.protocols.connectionContexts
{
	public class TcpConnectionContext : IConnectionContext
	{
		public TcpClient TcpClient { get; }

		public TcpConnectionContext(TcpClient tcpClient)
		{
			TcpClient = tcpClient;
		}

		public string Protocol => "tcp";
	}
}
