using System.Text.Json;
using BPMMessaging.models.entities;
using BPMMessaging.parsing;
using BPMMessaging.repository;
using Microsoft.Extensions.Logging;

namespace BPMMessaging.background.queuelistenersinfrastructure
{
	public class MessageProcessor : IMessageProcessor
	{
		private readonly IJsonParsingService _jsonParsingService;
		private readonly IMongoRepository<IncidentEntity> _incidentRepository;
		private readonly ILogger<MessageProcessor> _logger;

		public MessageProcessor(IJsonParsingService jsonParsingService,
								IMongoRepository<IncidentEntity> incidentRepository,
								ILogger<MessageProcessor> logger)
		{
			_jsonParsingService = jsonParsingService;
			_incidentRepository = incidentRepository;
			_logger = logger;
		}

		public async Task ProcessMessageAsync(string queueName, string message)
		{
			_logger.LogInformation($"Обработка сообщения из {queueName}: {message}");

			var jsonDocument = JsonDocument.Parse(message);
			var rootElement = jsonDocument.RootElement;

			// парсим полученное сообщение:
			IncidentEntity incident = _jsonParsingService.ParseJson<IncidentEntity>(rootElement);

			// делаем с сообщением что-то, обогащаем какими-то дополнительными данными, например:
			// ....=>

			// сохраняем полученное сообщение в таблицу инцидентов, если это нужно.
			await _incidentRepository.InsertAsync(incident);
			_logger.LogInformation($"Сообщение сохранено в БД как инцидент.");
		}
	}
}
