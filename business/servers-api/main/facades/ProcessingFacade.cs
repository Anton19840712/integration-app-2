using servers_api.models.internallayer.common;
using servers_api.models.response;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;
using System.Text.Json;

namespace servers_api.main.facades
{
	public class ProcessingFacade : IProcessingFacade
	{
		private readonly IJsonParsingService _jsonParsingService;
		private readonly ITeachService _teachService;
		private readonly ISenderService _senderService;

		public ProcessingFacade(IJsonParsingService jsonParsingService, ITeachService teachService, ISenderService senderService)
		{
			_jsonParsingService = jsonParsingService;
			_teachService = teachService;
			_senderService = senderService;
		}

		public async Task<CombinedModel> ParseJsonAsync(JsonElement jsonBody, bool isIntegration, CancellationToken stoppingToken)
			=> await _jsonParsingService.ParseJsonAsync(jsonBody, isIntegration, stoppingToken);

		public async Task<ResponseIntegration> ExecuteTeachAsync(CombinedModel model, CancellationToken stoppingToken)
			=> await _teachService.TeachBPMNAsync(model, stoppingToken);

		public async Task<ResponseIntegration> ConfigureNodeAsync(CombinedModel model, CancellationToken stoppingToken)
			=> await _senderService.UpAsync(model, stoppingToken);
	}
}
