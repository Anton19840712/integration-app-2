using servers_api.models.response;

namespace servers_api.handlers;

public interface ITeachHandler
{
	public List<ResponseIntegration> GenerateResultMessage(
			ResponseIntegration queueCreationTask = null,
			ResponseIntegration pushTask = null,
			ResponseIntegration receiveTask = null);
}
