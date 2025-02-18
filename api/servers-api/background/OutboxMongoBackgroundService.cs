using servers_api.repositories;

namespace servers_api.background;

public class OutboxMongoBackgroundService : BackgroundService
{
	private readonly IOutboxRepository _outboxRepository;
	private readonly IRabbitMqService _rabbitMqService;
	private readonly ILogger<OutboxMongoBackgroundService> _logger;

	public OutboxMongoBackgroundService(
		IOutboxRepository outboxRepository,
		IRabbitMqService rabbitMqService,
		ILogger<OutboxMongoBackgroundService> logger)
	{
		_outboxRepository = outboxRepository;
		_rabbitMqService = rabbitMqService;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken token)
	{
		_logger.LogInformation("OutboxMongoBackgroundService запущен.");

		// Параллельно запускаем фоновую очистку старых сообщений
		_ = Task.Run(() => CleanupOldMessagesAsync(token), token);

		while (!token.IsCancellationRequested)
		{
			try
			{
				var messages = await _outboxRepository.GetUnprocessedMessagesAsync();

				foreach (var message in messages)
				{
					_logger.LogInformation($"Публикация сообщения: {message.Message}.");

					await _rabbitMqService.PublishMessageAsync(message.InQueueName, message.RoutingKey, message.Message);
					await _outboxRepository.MarkMessageAsProcessedAsync(message.Id);

					_logger.LogInformation($"Обработано в Outbox: {message.Message}.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке Outbox.");
			}

			await Task.Delay(2000, token);
		}
	}

	private async Task CleanupOldMessagesAsync(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			try
			{
				int deletedCount = await _outboxRepository.DeleteOldMessagesAsync(TimeSpan.FromHours(24));
				_logger.LogInformation($"Удалено {deletedCount} старых сообщений из Outbox.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при очистке старых сообщений Outbox.");
			}

			// Запуск каждые 10 минут
			await Task.Delay(TimeSpan.FromMinutes(10), token);
		}
	}
}
