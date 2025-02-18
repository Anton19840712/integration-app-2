using servers_api.repositories;

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

			// Задержка между обработками сообщений, можно изменить на нужное значение
			await Task.Delay(2000, token);
		}
	}

	private async Task CleanupOldMessagesAsync(CancellationToken token)
	{
		const int ttlDifference = 10;  // Установите желаемый интервал сущестования объекта в базе данных.
		const int intervalInSeconds = 10;  // Установите желаемый интервал для повторной проверки сообщений, которые требуется удалить.

		while (!token.IsCancellationRequested)
		{
			try
			{
				int deletedCount = await _outboxRepository.DeleteOldMessagesAsync(TimeSpan.FromSeconds(ttlDifference));
				_logger.LogInformation($"Удалено {deletedCount} старых сообщений из Outbox.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при очистке старых сообщений Outbox.");
			}

			// Запуск очистки каждые intervalInSeconds
			await Task.Delay(TimeSpan.FromSeconds(intervalInSeconds), token);
		}
	}
}
