using System.Net.WebSockets;
using CommonGateLib.Connections;

namespace api.protocols.connectionContexts
{
	public class WebSocketConnectionContext : IConnectionContext
	{
		public WebSocket Socket { get; }

		public string Protocol => "websocket";

		public WebSocketConnectionContext(WebSocket socket) => Socket = socket;
	}
}


