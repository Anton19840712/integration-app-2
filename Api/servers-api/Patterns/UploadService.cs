using servers_api.Handlers;
using servers_api.Services.Brokers;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;
using System.Text.Json;

namespace servers_api.Patterns
{
    public class UploadService : IUploadService
    {
        private readonly ISenderService _senderService;
        private readonly IRabbitMqQueueListener _queueListener;
        private readonly IJsonParsingService _jsonParsingService;
        private readonly ITeachService _teachService;
        private readonly IRabbitMqQueueManager _rabbitMqQueueManager;
        private readonly IUploadHandler _uploadHandler;
        private readonly ILogger<UploadService> _logger;

        public UploadService(ISenderService senderService,
                                 IRabbitMqQueueListener queueListener,
                                 IJsonParsingService jsonParsingService,
                                 ITeachService teachService,
                                 IRabbitMqQueueManager rabbitMqQueueManager,
                                 IUploadHandler uploadHandler,
                                 ILogger<UploadService> logger)
        {
            _senderService = senderService;
            _queueListener = queueListener;
            _jsonParsingService = jsonParsingService;
            _teachService = teachService;
            _rabbitMqQueueManager = rabbitMqQueueManager;
            _uploadHandler = uploadHandler;
            _logger = logger;
        }

        public async Task<string> ConfigureAsync(JsonElement jsonBody, CancellationToken stoppingToken)
        {
            var parsedModel = _jsonParsingService.ParseJson(jsonBody);

            //var queueCreationTask = await _rabbitMqQueueManager.CreateQueues(parsedModel.InQueueName, parsedModel.OutQueueName);
            var senderConnectionTask = await _senderService.RunServerByProtocolTypeAsync(parsedModel, stoppingToken);
            //var apiStatusTask = await _teachService.TeachBPMNAsync(parsedModel, stoppingToken);
            //var receivedLastMessage = await _queueListener.StartListeningAsync(parsedModel.OutQueueName, stoppingToken);

            //var resultMessage = _uploadHandler.GenerateResultMessage(
            //                    queueCreationTask,
            //                    senderConnectionTask,
            //                    apiStatusTask,
            //                    receivedLastMessage);

            //return JsonSerializer.Serialize(resultMessage, _uploadHandler.GetJsonSerializerOptions());
            return default;
        }
    }
}
