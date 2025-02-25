using servers_api.services.brokers.bpmintegration;

namespace servers_api.main.facades
{
	public class QueueFacade : IQueueFacade
	{
		private readonly IRabbitMqQueueListener _queueListener;
		private readonly ILogger<QueueFacade> _logger;

		public QueueFacade(IRabbitMqQueueListener queueListener, ILogger<QueueFacade> logger)
		{
			_queueListener = queueListener;
			_logger = logger;
		}

		public async Task StartListeningAsync(string outQueue, CancellationToken stoppingToken)
		{
			await _queueListener.StartListeningAsync(outQueue, stoppingToken);
			_logger.LogInformation("Начато прослушивание очереди {OutQueue}", outQueue);
		}
	}
}
