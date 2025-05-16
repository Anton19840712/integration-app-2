
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonGateLib.Background
{
    public class OutboxMongoBackgroundService : BackgroundService
    {
        private readonly IMongoRepository<OutboxMessage> _outboxRepository;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<OutboxMongoBackgroundService> _logger;

        public OutboxMongoBackgroundService(
            IMongoRepository<OutboxMessage> outboxRepository,
            IRabbitMqService rabbitMqService,
            ILogger<OutboxMongoBackgroundService> logger)
        {
            _outboxRepository = outboxRepository;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("OutboxMongoBackgroundService: фоновый процесс по сканированию и отправке сообщений из outbox table запущен.");

            _ = Task.Run(() => CleanupOldMessagesAsync(token), token);
            _logger.LogInformation("OutboxMongoBackgroundService: фоновый процесс по ликвидации отправленных сообщений из outbox table запущен.");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var messages = await _outboxRepository.GetUnprocessedMessagesAsync();

                    foreach (var message in messages)
                    {
                        await _rabbitMqService.PublishMessageAsync(
                            message.InQueue,
                            message.RoutingKey,
                            message.Payload);

                        await _outboxRepository.MarkMessageAsProcessedAsync(message.Id);
                        _logger.LogInformation($"Обработано в Outbox: {message.Payload}");
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
            const int ttlDifference = 10;
            const int intervalInSeconds = 10;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    int deletedCount = await _outboxRepository.DeleteOldMessagesAsync(TimeSpan.FromSeconds(ttlDifference));
                    if (deletedCount != 0)
                    {
                        _logger.LogInformation($"OutboxMongoBackgroundService: yдалено {deletedCount} старых сообщений.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при очистке старых сообщений Outbox.");
                }

                await Task.Delay(TimeSpan.FromSeconds(intervalInSeconds), token);
            }
        }
    }
}
