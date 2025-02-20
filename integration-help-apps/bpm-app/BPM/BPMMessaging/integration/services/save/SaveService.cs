using System.Text.Json;
using BPMMessaging.integration.Enums;
using BPMMessaging.integration.Models;
using BPMMessaging.integration.services.parsing;
using MongoDB.Driver;


namespace BPMMessaging.integration.Services.Save
{
	public class SaveService : ISaveService
	{
		private readonly IMongoDatabase _database;
		private readonly IJsonParsingService _jsonParsingService;

		public SaveService(
			IMongoDatabase database,
			IJsonParsingService jsonParsingService)
		{
			_database = database;
			_jsonParsingService = jsonParsingService;
		}

		public async Task<IntegrationEntity> SaveIntegrationModelAsync(JsonElement jsonBody)
		{
			var parsedModel = _jsonParsingService.ParseJson(jsonBody);

			var integrationCollection = _database.GetCollection<IntegrationEntity>("IntegrationEntities");
			var outboxCollection = _database.GetCollection<OutboxMessage>("OutboxMessages");

			var outboxMessage = new OutboxMessage
			{
				Id = Guid.NewGuid(),
				EventType = EventTypes.Created,
				Payload = parsedModel,
				IsProcessed = false,
				OutQueue = parsedModel.OutQueueName,
				CreatedAt = DateTime.UtcNow
			};

			var saveParsedModelTask = integrationCollection.InsertOneAsync(parsedModel);
			var saveOutboxMessageTask = outboxCollection.InsertOneAsync(outboxMessage);

			await Task.WhenAll(saveParsedModelTask, saveOutboxMessageTask);

			return parsedModel;
		}
	}
}
