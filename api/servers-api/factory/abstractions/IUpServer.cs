using servers_api.models.internallayerusage.instance;
using servers_api.models.responce;

namespace servers_api.factory.abstractions
{
	public interface IUpServer
	{
		Task<ResponceIntegration> UpServerAsync(
			ServerInstanceModel instanceModel,
			CancellationToken cancellationToken);
	}
}
