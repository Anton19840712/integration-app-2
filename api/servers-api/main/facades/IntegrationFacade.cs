using servers_api.models.internallayer.common;
using servers_api.models.response;
using System.Text.Json;

namespace servers_api.main.facades
{
	public class IntegrationFacade(IQueueFacade queueFacade, IMessageFacade messageFacade, IProcessingFacade processingFacade) : IIntegrationFacade
	{
		public Task<CombinedModel> ParseJsonAsync(JsonElement jsonBody, bool isIntegration, CancellationToken stoppingToken)
			=> processingFacade.ParseJsonAsync(jsonBody, isIntegration, stoppingToken);

		public Task<ResponseIntegration> CreateQueuesAsync(string inQueue, string outQueue, CancellationToken stoppingToken)
			=> queueFacade.CreateQueuesAsync(inQueue, outQueue, stoppingToken);

		public Task StartListeningAsync(string outQueue, CancellationToken stoppingToken)
			=> queueFacade.StartListeningAsync(outQueue, stoppingToken);

		public Task<ResponseIntegration> GetLastMessageAsync(CancellationToken stoppingToken)
			=> messageFacade.GetLastMessageAsync(stoppingToken);

		public Task<ResponseIntegration> TeachBpmAsync(CombinedModel model, CancellationToken stoppingToken)
			=> processingFacade.ExecuteTeachAsync(model, stoppingToken);

		public Task<ResponseIntegration> ConfigureNodeAsync(CombinedModel model, CancellationToken stoppingToken)
			=> processingFacade.ConfigureNodeAsync(model, stoppingToken);
	}
}
