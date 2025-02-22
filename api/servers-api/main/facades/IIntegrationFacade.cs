using System.Text.Json;
using servers_api.models.internallayer.common;
using servers_api.models.response;

namespace servers_api.main.facades
{
	public interface IIntegrationFacade
	{
		Task<CombinedModel> ParseJsonAsync(JsonElement jsonBody, bool isIntegration, CancellationToken stoppingToken);
		Task<ResponseIntegration> CreateQueuesAsync(string inQueue, string outQueue, CancellationToken stoppingToken);
		Task StartListeningAsync(string outQueue, CancellationToken stoppingToken);
		Task<ResponseIntegration> GetLastMessageAsync(CancellationToken stoppingToken);
		Task<ResponseIntegration> TeachBpmAsync(CombinedModel model, CancellationToken stoppingToken);
		Task<ResponseIntegration> ConfigureNodeAsync(CombinedModel model, CancellationToken stoppingToken);
	}
}
