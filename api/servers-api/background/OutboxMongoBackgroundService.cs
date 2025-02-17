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
		// Этот метод будет выполняться в фоновом потоке
		while (!token.IsCancellationRequested)
		{
			try
			{
				// получаем сообщения из базы данных, которые мы приземлили от нашего клиента, если мы сервер:
				// получаем сообщения, которые не были запроцешены:
				var messages = await _outboxRepository.GetUnprocessedMessagesAsync();
				foreach (var message in messages)
				{
					_logger.LogInformation($"Публикация сообщения: {message.Message}.");

					// Отправляем сообщения в сетевую шину rabbitMQ:
					await _rabbitMqService.PublishMessageAsync("exchange_name_tcp", "routing_key_tcp", message.Message);

					// Помечаем сообщение запроцешенными: isProcessed устанавливаем в true:
					// TODO проанализируй, если сообщений накопиться очень много, что будет.
					await _outboxRepository.MarkMessageAsProcessedAsync(message.Id);
					_logger.LogInformation($"Обработано в Outbox: {message.Message}.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке Outbox.");
			}

			// Ожидаем 2 секунды перед следующим циклом проверки таблицы, которая содержит outbox сообщения
			await Task.Delay(2000, token);
		}
	}
}
