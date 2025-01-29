using servers_api.models;

namespace servers_api.factory.abstractions
{
	public interface IUpClient
	{
	    Task<ResponceIntegration> ConnectToServerAsync(string host, int port);
	}
}
