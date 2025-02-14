using System.Text.Json;
using servers_api.handlers;
using servers_api.main.facades;
using servers_api.models.response;

namespace servers_api.main.services;

public class StartNodeService : IStartNodeService
{
	private readonly IIntegrationFacade _integrationFacade;
	private readonly ITeachHandler _uploadHandler;
	private readonly ILogger<StartNodeService> _logger;

	public StartNodeService(
		IIntegrationFacade integrationFacade,
		ITeachHandler uploadHandler,
		ILogger<StartNodeService> logger)
	{
		_integrationFacade = integrationFacade;
		_uploadHandler = uploadHandler;
		_logger = logger;
	}

	public async Task<List<ResponseIntegration>> ConfigureNodeAsync(JsonElement jsonBody, CancellationToken stoppingToken)
	{
		_logger.LogInformation("Начало обработки ConfigureNodeAsync");

		var parsedModel = await _integrationFacade.ParseJsonAsync(jsonBody, stoppingToken);
		var apiStatus = await _integrationFacade.ConfigureNodeAsync(parsedModel, stoppingToken);

		var result = _uploadHandler.GenerateResultMessage(null, apiStatus, null);

		_logger.LogInformation("Завершение ConfigureNodeAsync");
		return result;
	}
}
