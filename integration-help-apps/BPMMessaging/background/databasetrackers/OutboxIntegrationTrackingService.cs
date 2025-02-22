using MongoDB.Driver;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BPMMessaging.models.dtos;
using BPMMessaging.publishing;
using Microsoft.Extensions.DependencyInjection;

namespace BPMIntegration.Services.Background
{
	/// <summary>
	/// Данный сервис целенаправленно каждые 5000 мс собирает информацию из outbox table на предмет того, запроцешенно ли сообщение или нет.
	/// После этого публикует незапроцешенные сообщение в out очередь, которую слушает динамический шлюз с другой стороны.
	/// Далее сообщение заслушается на стороне сервера интеграционного шлюза из очереди реббит шины и выведется в консоль
	/// </summary>
	namespace BPMIntegration.Services.Background
	{
		public class OutboxIntegrationTrackingService : IHostedService
		{
			private readonly IServiceScopeFactory _serviceScopeFactory;
			private readonly ILogger<OutboxIntegrationTrackingService> _logger;

			public OutboxIntegrationTrackingService(
				IServiceScopeFactory serviceScopeFactory,
				ILogger<OutboxIntegrationTrackingService> logger)
			{
				_serviceScopeFactory = serviceScopeFactory;
				_logger = logger;
			}

			public Task StartAsync(CancellationToken cancellationToken)
			{
				_logger.LogInformation("OutboxProcessorService запущен.");

				_ = Task.Run(async () =>
				{
					while (!cancellationToken.IsCancellationRequested)
					{
						try
						{
							await ProcessOutboxMessagesAsync(cancellationToken);
						}
						catch (OperationCanceledException)
						{
							_logger.LogInformation("Обработка сообщений была отменена.");
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Ошибка в процессе обработки сообщений.");
						}

						await Task.Delay(5000, cancellationToken);
					}
				}, cancellationToken);

				return Task.CompletedTask;
			}

			public Task StopAsync(CancellationToken cancellationToken)
			{
				_logger.LogInformation("OutboxProcessorService остановлен.");
				return Task.CompletedTask;
			}

			private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var mongoDatabase = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
				var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

				var collection = mongoDatabase.GetCollection<OutboxMessage>("OutboxMessages");

				try
				{
					var filter = Builders<OutboxMessage>.Filter.Eq(m => m.IsProcessed, false);
					var outboxMessages = await collection.Find(filter).ToListAsync(cancellationToken);

					if (outboxMessages.Any())
					{
						_logger.LogInformation($"Найдено {outboxMessages.Count} сообщений для обработки.");

						foreach (var message in outboxMessages)
						{
							cancellationToken.ThrowIfCancellationRequested();

							await messagePublisher.PublishAsync(message.OutQueue, message);

							var update = Builders<OutboxMessage>.Update.Set(m => m.IsProcessed, true);
							await collection.UpdateOneAsync(Builders<OutboxMessage>.Filter.Eq(m => m.Id, message.Id), update);

							_logger.LogInformation($"Сообщение outbox {message.Id} отправлено в очередь {message.OutQueue}.");
						}

						_logger.LogInformation("Все сообщения успешно обработаны.");
					}
					else
					{
						_logger.LogInformation("Очередь in пуста, обработка пропущена.");
					}
				}
				catch (OperationCanceledException)
				{
					_logger.LogInformation("Обработка сообщений была отменена.");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке сообщений в очереди.");
				}
			}
		}
	}
}
