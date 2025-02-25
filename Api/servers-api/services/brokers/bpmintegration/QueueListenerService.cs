using RabbitMQ.Client;
using servers_api.models.entities;
using servers_api.repositories;
using servers_api.services.brokers.bpmintegration;

public class QueueListenerService
{
	private readonly IConnectionFactory _connectionFactory;
	private readonly ILoggerFactory _loggerFactory;
	private readonly MongoRepository<QueuesEntity> _queuesRepository;

	public QueueListenerService(
		IConnectionFactory connectionFactory,
		ILoggerFactory loggerFactory,
		MongoRepository<QueuesEntity> queuesRepository)
	{
		_connectionFactory = connectionFactory;
		_loggerFactory = loggerFactory;
		_queuesRepository = queuesRepository;
	}

	public async Task<List<RabbitMqQueueListener>> StartQueueListenersAsync(
		CancellationToken cancellationToken)
	{
		var queueEntities = await _queuesRepository.GetAllAsync();
		var consumers = new List<RabbitMqQueueListener>();

		foreach (var queueEntity in queueEntities)
		{
			var queueInName = queueEntity.InQueueName;
			var queueOutName = queueEntity.OutQueueName;

			// оформить в виде иньекции:
			var logger = _loggerFactory.CreateLogger<RabbitMqQueueListener>();

			// оформить в виде иньекции:
			var listener = new RabbitMqQueueListener(
				_connectionFactory,
				logger);
			await listener.StartListeningAsync(queueOutName, cancellationToken);
			consumers.Add(listener);
		}

		return consumers;
	}
}
