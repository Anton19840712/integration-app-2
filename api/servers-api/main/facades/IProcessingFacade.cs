using servers_api.models.internallayer.common;
using servers_api.models.response;
using System.Text.Json;

namespace servers_api.main.facades
{
	public interface IProcessingFacade
	{
		Task<CombinedModel> ParseJsonAsync(JsonElement jsonBody, CancellationToken stoppingToken);
		Task<ResponseIntegration> ExecuteTeachAsync(CombinedModel model, CancellationToken stoppingToken);
		Task<ResponseIntegration> ConfigureNodeAsync(CombinedModel model, CancellationToken stoppingToken);
	}
}
