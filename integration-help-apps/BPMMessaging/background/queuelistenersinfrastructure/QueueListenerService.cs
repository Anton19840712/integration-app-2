using BPMMessaging.models.dtos;
using BPMMessaging.models.entities;
using BPMMessaging.repository;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

public class QueueListenerService
{
	private readonly IConnectionFactory _connectionFactory;
	private readonly ILoggerFactory _loggerFactory;
	private readonly IMongoRepository<TeachingEntity> _teachingRepository;
	private readonly IMongoRepository<IncidentEntity> _incidentRepository;
	private readonly IMongoRepository<OutboxMessage> _outboxRepository;

	public QueueListenerService(
		IConnectionFactory connectionFactory,
		ILoggerFactory loggerFactory,
		IMongoRepository<TeachingEntity> teachingRepository,
		IMongoRepository<IncidentEntity> incidentRepository,
		IMongoRepository<OutboxMessage> outboxRepository)
	{
		_connectionFactory = connectionFactory;
		_loggerFactory = loggerFactory;
		_teachingRepository = teachingRepository;
		_incidentRepository = incidentRepository;
		_outboxRepository = outboxRepository;
	}

	public async Task<List<RabbitMqQueueListener>> StartQueueListenersAsync(CancellationToken cancellationToken)
	{
		var teachingEntities = await _teachingRepository.GetAllAsync();
		var consumers = new List<RabbitMqQueueListener>();

		foreach (var teachingEntity in teachingEntities)
		{
			var queueInName = teachingEntity.InQueueName;
			var queueOutName = teachingEntity.OutQueueName;

			// оформить в виде иньекции:
			var logger = _loggerFactory.CreateLogger<RabbitMqQueueListener>();

			// оформить в виде иньекции:
			var listener = new RabbitMqQueueListener(_connectionFactory, logger, _incidentRepository, _outboxRepository);
			await listener.StartListeningAsync(queueInName, queueOutName, cancellationToken);
			consumers.Add(listener);
		}

		return consumers;
	}
}
