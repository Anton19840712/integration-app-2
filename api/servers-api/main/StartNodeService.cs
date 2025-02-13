using System.Text.Json;
using servers_api.handlers;
using servers_api.models.response;
using servers_api.Services.Connectors;
using servers_api.Services.Parsers;

namespace servers_api.main;

public class StartNodeService : IStartNodeService
{
	private readonly IJsonParsingService _jsonParsingService;
	private readonly ISenderService _senderService;
	private readonly ILogger<StartNodeService> _logger;
	private readonly ITeachHandler _uploadHandler;

	public StartNodeService(
		IJsonParsingService jsonParsingService,
		ISenderService senderService,
		ITeachHandler uploadHandler,
		ILogger<StartNodeService> logger)
	{
		_jsonParsingService = jsonParsingService;
		_senderService = senderService;
		_logger = logger;
		_uploadHandler = uploadHandler;
	}

	public async Task<List<ResponseIntegration>> ConfigureNodeAsync(
		JsonElement jsonBody,
		CancellationToken stoppingToken)
	{
		var parsedModel = _jsonParsingService.ParseJson(jsonBody);

		var senderConnectionTask = await _senderService.UpAsync(
			parsedModel,
			stoppingToken);

		var result = _uploadHandler.GenerateResultMessage(
							null,
							null,
							null);
		return result;
	}
}
