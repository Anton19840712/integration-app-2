using servers_api.services.brokers.bpmintegration;
using servers_api.services.brokers.tcprest;

namespace servers_api.main.facades
{
	public class QueueFacade : IQueueFacade
	{
		private readonly IRabbitQueuesCreator _rabbitQueueManager;
		private readonly IRabbitMqQueueListener _queueListener;
		private readonly ILogger<QueueFacade> _logger;

		public QueueFacade(IRabbitQueuesCreator rabbitQueueManager, IRabbitMqQueueListener queueListener, ILogger<QueueFacade> logger)
		{
			_rabbitQueueManager = rabbitQueueManager;
			_queueListener = queueListener;
			_logger = logger;
		}

		public async Task CreateQueuesAsync(string inQueue, string outQueue, CancellationToken stoppingToken)
		{
			await _rabbitQueueManager.CreateQueuesAsync(inQueue, outQueue, stoppingToken);
			_logger.LogInformation("Очереди {InQueue} и {OutQueue} созданы", inQueue, outQueue);
		}

		public async Task StartListeningAsync(string outQueue, CancellationToken stoppingToken)
		{
			await _queueListener.StartListeningAsync(outQueue, stoppingToken);
			_logger.LogInformation("Начато прослушивание очереди {OutQueue}", outQueue);
		}
	}
}
