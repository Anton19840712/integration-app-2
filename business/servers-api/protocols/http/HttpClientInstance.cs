using servers_api.factory;
using servers_api.models.internallayer.instance;
using servers_api.models.response;

namespace servers_api.protocols.http
{
	public class HttpClientInstance : IUpClient
	{
		public Task<ResponseIntegration> ConnectToServerAsync(
			ClientInstanceModel instanceModel,
			string serverHost,
			int serverPort,
			CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
