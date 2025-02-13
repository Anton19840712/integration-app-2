using System.Text.Json;
using servers_api.models.response;

namespace servers_api.main;

public interface IStartNodeService
{
	Task<List<ResponseIntegration>> ConfigureNodeAsync(
		JsonElement jsonBody,
		CancellationToken stoppingToke);
}
