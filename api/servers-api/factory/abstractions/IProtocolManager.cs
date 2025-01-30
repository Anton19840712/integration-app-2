using servers_api.models.internallayerusage.instance;
using servers_api.models.responce;

namespace servers_api.factory.abstractions
{
	public interface IProtocolManager
	{
		Task<ResponceIntegration> ConfigureAsync(InstanceModel instanceModel);
	}
}
