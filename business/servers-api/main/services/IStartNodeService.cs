using System.Text.Json;
using servers_api.models.response;

namespace servers_api.main.services;

public interface IStartNodeService
{
	Task<ResponseIntegration> ConfigureNodeAsync(
		JsonElement jsonBody,
		CancellationToken stoppingToke);
}
