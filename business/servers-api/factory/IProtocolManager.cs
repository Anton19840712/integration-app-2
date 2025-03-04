using servers_api.models.internallayer.instance;
using servers_api.models.response;

namespace servers_api.factory;

public interface IProtocolManager
{
	Task<ResponseIntegration> ConfigureAsync(InstanceModel instanceModel);
}
