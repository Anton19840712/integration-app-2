using servers_api.main.facades;
using servers_api.models.entities;
using servers_api.repositories;

public class QueueListenerBackgroundService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<QueueListenerBackgroundService> _logger;

	public QueueListenerBackgroundService(
		IServiceScopeFactory scopeFactory,
		ILogger<QueueListenerBackgroundService> logger)
	{
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Запуск фоново сервиса слушателей очередей.");

		try
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var integrationFacade = scope.ServiceProvider.GetRequiredService<IIntegrationFacade>();
				var queuesRepository = scope.ServiceProvider.GetRequiredService<MongoRepository<QueuesEntity>>();

				var elements = await queuesRepository.GetAllAsync();

				// Для каждой очереди запускаем слушателя в отдельном потоке
				foreach (var element in elements)
				{
					try
					{
						// Слушаем очередь в фоне
						await Task.Run(() => StartListeningAsync(integrationFacade, element.OutQueueName, stoppingToken), stoppingToken);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Ошибка при запуске слушателя для очереди: {QueueName}", element.OutQueueName);
					}
				}

				_logger.LogInformation("Процесс получения сообщений из очередей завершен.");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при получении очередей для начала прослушивания");
		}
	}

	private async Task StartListeningAsync(IIntegrationFacade integrationFacade, string queueOutName, CancellationToken stoppingToken)
	{
		try
		{
			_logger.LogInformation($"Подключаемся к очереди {queueOutName}");
			// Логика слушателя очереди
			await integrationFacade.StartListeningAsync(queueOutName, stoppingToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при прослушивании очереди: {QueueName}", queueOutName);
		}
	}
}
