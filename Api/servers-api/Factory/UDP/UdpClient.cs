using servers_api.Factory.Abstractions;

namespace servers_api.Factory.UDP
{
	    public class UdpClient : IClient
	    {
	        public void ConnectToServer(string host, int port)
	        {
	            //TODO
	            //HACK
	            //FIXME
	            //CRUTCH
	            Console.WriteLine($"UDP Client: Connecting to {host}:{port}");
	        }
	    }
}
