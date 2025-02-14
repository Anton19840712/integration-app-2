using servers_api.models.internallayer.common;
using servers_api.models.response;
using System.Text.Json;

namespace servers_api.main.facades
{
	public class IntegrationFacade : IIntegrationFacade
	{
		private readonly IQueueFacade _queueFacade;
		private readonly IMessageFacade _messageFacade;
		private readonly IProcessingFacade _processingFacade;

		public IntegrationFacade(IQueueFacade queueFacade, IMessageFacade messageFacade, IProcessingFacade processingFacade)
		{
			_queueFacade = queueFacade;
			_messageFacade = messageFacade;
			_processingFacade = processingFacade;
		}

		public Task<CombinedModel> ParseJsonAsync(JsonElement jsonBody, bool isIntegration, CancellationToken stoppingToken)
			=> _processingFacade.ParseJsonAsync(jsonBody, isIntegration, stoppingToken);

		public Task CreateQueuesAsync(string inQueue, string outQueue, CancellationToken stoppingToken)
			=> _queueFacade.CreateQueuesAsync(inQueue, outQueue, stoppingToken);

		public Task StartListeningAsync(string outQueue, CancellationToken stoppingToken)
			=> _queueFacade.StartListeningAsync(outQueue, stoppingToken);

		public Task<ResponseIntegration> GetLastMessageAsync(CancellationToken stoppingToken)
			=> _messageFacade.GetLastMessageAsync(stoppingToken);

		public Task<ResponseIntegration> ExecuteTeachAsync(CombinedModel model, CancellationToken stoppingToken)
			=> _processingFacade.ExecuteTeachAsync(model, stoppingToken);

		public Task<ResponseIntegration> ConfigureNodeAsync(CombinedModel model, CancellationToken stoppingToken)
			=> _processingFacade.ConfigureNodeAsync(model, stoppingToken);
	}
}
