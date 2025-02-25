using servers_api.services.brokers.bpmintegration;

namespace servers_api.main.facades
{
	public class QueueFacade : IQueueFacade
	{
		private readonly IRabbitMqQueueListener _queueListener;

		public QueueFacade(IRabbitMqQueueListener queueListener)
		{
			_queueListener = queueListener;
		}

		public async Task StartListeningAsync(string outQueue, CancellationToken stoppingToken)
		{
			await _queueListener.StartListeningAsync(outQueue, stoppingToken);
		}
	}
}
