using System.Text.Json;
using servers_api.handlers;
using servers_api.main.facades;
using servers_api.models.response;

namespace servers_api.main.services;

/// <summary>
/// Общий менеджер-сервис, занимающийся процессингом настройки
/// всей инфраструктуры динамического шлюза под отдельную организацию.
/// </summary>
public class TeachIntegrationService : ITeachIntegrationService
{
	private readonly IIntegrationFacade _integrationFacade;
	private readonly ITeachHandler _uploadHandler;
	private readonly ILogger<TeachIntegrationService> _logger;

	public TeachIntegrationService(
		IIntegrationFacade integrationFacade,
		ITeachHandler uploadHandler,
		ILogger<TeachIntegrationService> logger)
	{
		_integrationFacade = integrationFacade;
		_uploadHandler = uploadHandler;
		_logger = logger;
	}

	public async Task<List<ResponseIntegration>> TeachAsync(JsonElement jsonBody, CancellationToken stoppingToken)
	{
		_logger.LogInformation("Начало обработки TeachAsync");

		var parsedModel = await _integrationFacade.ParseJsonAsync(jsonBody, stoppingToken);
		await _integrationFacade.CreateQueuesAsync(parsedModel.InQueueName, parsedModel.OutQueueName, stoppingToken);

		var apiStatus = await _integrationFacade.ExecuteTeachAsync(parsedModel, stoppingToken);
		await _integrationFacade.StartListeningAsync(parsedModel.OutQueueName, stoppingToken);

		var receivedMessage = await _integrationFacade.GetLastMessageAsync(stoppingToken);
		var result = _uploadHandler.GenerateResultMessage(null, apiStatus, receivedMessage);

		_logger.LogInformation("Завершение TeachAsync");
		return result;
	}
}
