using System.Text.Json;
using servers_api.handlers;
using servers_api.models.response;
using servers_api.services.brokers.bpmintegration;
using servers_api.services.brokers.tcprest;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;

namespace servers_api.main;

/// <summary>
/// Общий менеджер-сервис, занимающийся процессингом настройки
/// всей инфраструктуры динамического шлюза под отдельную организацию.
/// </summary>
public class TeachIntegrationService : ITeachIntegrationService
{
	private readonly IRabbitMqQueueListener _queueListener;
	private readonly IJsonParsingService _jsonParsingService;
	private readonly ITeachIntegrationService _teachService;
	private readonly IRabbitQueuesCreator _rabbitMqQueueManager;
	private readonly ITeachHandler _uploadHandler;
	private readonly ILogger<TeachIntegrationService> _logger;

	public TeachIntegrationService(IRabbitMqQueueListener queueListener,
						IJsonParsingService jsonParsingService,
						ITeachIntegrationService teachService,
						IRabbitQueuesCreator rabbitMqQueueManager,
						ITeachHandler uploadHandler,
						ILogger<TeachIntegrationService> logger)
	{
		
		_queueListener = queueListener;
		_jsonParsingService = jsonParsingService;
		_teachService = teachService;
		_rabbitMqQueueManager = rabbitMqQueueManager;
		_uploadHandler = uploadHandler;
		_logger = logger;
	}

	public async Task<List<ResponseIntegration>> TeachAsync(
		JsonElement jsonBody,
		CancellationToken stoppingToken)
	{
		var parsedModel = _jsonParsingService.ParseJson(jsonBody);

		//var queueCreationTask = await _rabbitMqQueueManager.CreateQueues(parsedModel.InQueueName, parsedModel.OutQueueName);

		//var apiStatusTask = await _teachService.TeachBPMNAsync(parsedModel, stoppingToken);


		//var receivedLastMessage = await _queueListener.StartListeningAsync(parsedModel.OutQueueName, stoppingToken);


		var result = _uploadHandler.GenerateResultMessage(
							null,
							null,
							null);
		return result;
	}
}
