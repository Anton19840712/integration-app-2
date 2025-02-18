using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BPMIntegration.Models;
using BPMIntegration.Publishing;

namespace BPMIntegration.Services.Background
{
	/// <summary>
	/// Данный сервис целенаправленно каждые 5000 мс собирает информацию из outbox table
	/// После этого публикует сообщение в очередь, которую слушает динамический шлюз
	/// Что такое-то сообщение было получено и зафиксировано на стороне bpm
	/// Это сообщение залогируется на стороне интеграционной шины, что будет являться признаком того,
	/// что настроечное сообщение было обработано.
	/// </summary>
	namespace BPMIntegration.Services.Background
	{
		public class OutboxIntegrationProcessorService : IHostedService
		{
			private readonly IServiceScopeFactory _serviceScopeFactory;
			private readonly ILogger _logger;

			public OutboxIntegrationProcessorService(IServiceScopeFactory serviceScopeFactory, ILogger logger)
			{
				_serviceScopeFactory = serviceScopeFactory;
				_logger = logger;
			}

			public Task StartAsync(CancellationToken cancellationToken)
			{
				_logger.Information("OutboxProcessorService запущен.");

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
							_logger.Information("Обработка сообщений была отменена.");
						}
						catch (Exception ex)
						{
							_logger.Error(ex, "Ошибка в процессе обработки сообщений.");
						}

						await Task.Delay(5000, cancellationToken);
					}
				}, cancellationToken);

				return Task.CompletedTask;
			}

			public Task StopAsync(CancellationToken cancellationToken)
			{
				_logger.Information("OutboxProcessorService остановлен.");
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
						_logger.Information("Найдено {Count} сообщений для обработки.", outboxMessages.Count);

						foreach (var message in outboxMessages)
						{
							cancellationToken.ThrowIfCancellationRequested();

							await messagePublisher.PublishAsync(message.OutQueue, message.Payload);

							var update = Builders<OutboxMessage>.Update.Set(m => m.IsProcessed, true);
							await collection.UpdateOneAsync(Builders<OutboxMessage>.Filter.Eq(m => m.Id, message.Id), update);

							_logger.Information(
								"Сообщение outbox {MessageId} c incoming model id {Id} отправлено в очередь {Queue}.",
								message.Id,
								message.Payload.Id,
								message.OutQueue);
						}

						_logger.Information("Все сообщения успешно обработаны.");
					}
					else
					{
						_logger.Debug("Очередь сообщений пуста, обработка пропущена.");
					}
				}
				catch (OperationCanceledException)
				{
					_logger.Information("Обработка сообщений была отменена.");
				}
				catch (Exception ex)
				{
					_logger.Error(ex, "Ошибка при обработке сообщений в очереди.");
				}
			}
		}
	}
}
