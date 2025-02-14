using servers_api.models.response;
using servers_api.services.brokers.bpmintegration;

namespace servers_api.main.facades
{
	public class MessageFacade : IMessageFacade
	{
		private readonly IRabbitMqQueueListener _queueListener;

		public MessageFacade(IRabbitMqQueueListener queueListener)
		{
			_queueListener = queueListener;
		}

		public async Task<ResponseIntegration> GetLastMessageAsync(CancellationToken stoppingToken)
			=> (await _queueListener.GetCollectedMessagesAsync(stoppingToken)).FirstOrDefault();
	}
}
