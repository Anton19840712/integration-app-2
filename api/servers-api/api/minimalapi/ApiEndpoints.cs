using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using servers_api.main.facades;
using servers_api.main.services;
using servers_api.models.entities;
using servers_api.repositories;

namespace servers_api.api.minimalapi;

public static class ApiEndpoints
{
	public static void MapCommonApiEndpoints(
		this IEndpointRouteBuilder app,
		ILoggerFactory loggerFactory)
	{
		var logger = loggerFactory.CreateLogger("ApiEndpoints");

		// POST-запрос для конфигурации системы интеграции
		app.MapPost("/api/servers/teach", async (
			[FromBody] JsonElement jsonBody,
			ITeachIntegrationService uploadFileService,
			CancellationToken stoppingToken) =>
		{
			try
			{
				logger.LogInformation("Upload endpoint called with body: {JsonBody}", jsonBody.ToString());
				var result = await uploadFileService.TeachAsync(jsonBody, stoppingToken);
				logger.LogInformation("File uploaded successfully");
				return Results.Ok(result);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error during file upload");
				return Results.Problem(ex.Message);
			}
		});

		// POST-запрос для старта инстанса нашей системы
		app.MapPost("/api/servers/run", async (
			[FromBody] JsonElement jsonBody,
			IStartNodeService startNodeService,
			CancellationToken stoppingToken) =>
		{
			try
			{
				logger.LogInformation("Start server endpoint called with body: {JsonBody}", jsonBody.ToString());
				var result = await startNodeService.ConfigureNodeAsync(jsonBody, stoppingToken);
				logger.LogInformation("Node configured successfully");
				return Results.Ok(result);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error during file upload");
				return Results.Problem(ex.Message);
			}
		});

		app.MapGet("/api/servers/messages", async (
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

				logger.LogInformation("Messages retrieved successfully");
				return Results.Ok();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error while getting messages from queues");
				return Results.Problem(ex.Message);
			}
		});
	}
}
