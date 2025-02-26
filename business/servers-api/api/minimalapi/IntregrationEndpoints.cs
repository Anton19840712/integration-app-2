using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using servers_api.main.services;

namespace servers_api.api.minimalapi;

public static class IntregrationEndpoints
{
	public static void MapIntegrationMinimalApis(
		this IEndpointRouteBuilder app,
		ILoggerFactory loggerFactory)
	{
		var logger = loggerFactory.CreateLogger("IntregrationEndpoints");

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
	}
}
