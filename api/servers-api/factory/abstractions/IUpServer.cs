using servers_api.models.responce;

namespace servers_api.factory.abstractions
{
	public interface IUpServer
	{
	    Task<ResponceIntegration> UpServerAsync(
			string host,
			int? port,
			CancellationToken cancellationToken);
	}
}
