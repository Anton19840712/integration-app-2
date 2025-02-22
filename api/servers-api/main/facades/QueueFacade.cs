using servers_api.models.response;
using servers_api.services.brokers.bpmintegration;
using servers_api.services.brokers.tcprest;

namespace servers_api.main.facades
{
	public class QueueFacade(IRabbitQueuesCreator rabbitQueueManager, IRabbitMqQueueListener queueListener, ILogger<QueueFacade> logger) : IQueueFacade
	{
		public async Task<ResponseIntegration> CreateQueuesAsync(string inQueue, string outQueue, CancellationToken stoppingToken)
		{
			var resultOfCreation = await rabbitQueueManager.CreateQueuesAsync(inQueue, outQueue, stoppingToken);
			logger.LogInformation("Очереди {InQueue} и {OutQueue} созданы", inQueue, outQueue);
			return resultOfCreation;
		}

		public async Task StartListeningAsync(string outQueue, CancellationToken stoppingToken)
		{
			await queueListener.StartListeningAsync(outQueue, stoppingToken);
			logger.LogInformation("Начато прослушивание очереди {OutQueue}", outQueue);
		}
	}
}
