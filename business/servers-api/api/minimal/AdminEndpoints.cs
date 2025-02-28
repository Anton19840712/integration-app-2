using servers_api.main.facades;
using servers_api.models.entities;
using servers_api.repositories;

namespace servers_api.api.minimal;

public static class AdminEndpoints
{
	public static void MapAdminMinimalApi(
		this IEndpointRouteBuilder app,
		ILoggerFactory loggerFactory)
	{
		var logger = loggerFactory.CreateLogger("AdminEndpoints");

		app.MapGet("/api/consume", async (
			IIntegrationFacade integrationFacade,
			MongoRepository<QueuesEntity> queuesRepository,
			CancellationToken stoppingToken) =>
		{
			try
			{
				logger.LogInformation("Dumping messages from all queues.");

				// Получаем названия всех очередей из репозитория:
				var elements = await queuesRepository.GetAllAsync();

				foreach (var element in elements)
				{
					try
					{
						// Для каждой очереди запускаем слушателя в отдельной задаче:
						await integrationFacade.StartListeningAsync(element.OutQueueName, stoppingToken);
					}
					catch (Exception ex)
					{
						// Логируем ошибку для каждой очереди отдельно, но продолжаем обработку других:
						logger.LogError(ex, "Error retrieving messages from queue: {QueueName}", element.OutQueueName);
					}
				}

				logger.LogInformation("Процесс получения сообщений из очередей завершен.");
				return Results.Ok();
			}


			catch (Exception ex)
			{
				logger.LogError(ex, "Error while getting messages from queues");
				return Results.Problem(ex.Message);
			}
		});

		// Тестовый эндпоинт /api/ping
		app.MapGet("/api/ping", (HttpContext context) =>
		{
			Console.WriteLine($"Ping requested from {context.Connection.RemoteIpAddress}");
			return Results.Ok("Hello, world!");
		});
	}
}
