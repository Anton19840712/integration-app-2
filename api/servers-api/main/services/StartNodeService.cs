using System.Text.Json;
using servers_api.main.facades;
using servers_api.models.response;

namespace servers_api.main.services;

public class StartNodeService(
	IIntegrationFacade integrationFacade,
	ILogger<StartNodeService> logger) : IStartNodeService
{
	public async Task<ResponseIntegration> ConfigureNodeAsync(JsonElement jsonBody, CancellationToken stoppingToken)
	{
		logger.LogInformation("Начало обработки ConfigureNodeAsync");

		var parsedModel = await integrationFacade.ParseJsonAsync(jsonBody, false, stoppingToken);
		var apiStatus = await integrationFacade.ConfigureNodeAsync(parsedModel, stoppingToken);

		logger.LogInformation("Завершение ConfigureNodeAsync");
		return apiStatus;
	}
}
