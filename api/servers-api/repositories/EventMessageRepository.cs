using MongoDB.Driver;
using servers_api.events;

namespace servers_api.repositories;
public class EventMessageRepository : IEventMessageRepository
{
	private readonly IMongoCollection<EventMessage> _eventsCollection;

	public EventMessageRepository(IMongoDatabase database, IConfiguration configuration)
	{

		string collectionName = configuration.GetValue<string>("MongoDbSettings:Collections:EventCollection") ?? "IntegrationEvents";
		_eventsCollection = database.GetCollection<EventMessage>(collectionName);
	}

	public async Task SaveEventMessageAsync(EventMessage eventMessage)
	{
		try
		{
			await _eventsCollection.InsertOneAsync(eventMessage);
		}
		catch (Exception ex)
		{
			// Логирование ошибки (например, через Serilog)
			throw new Exception("Ошибка при сохранении сообщения в MongoDB", ex);
		}
	}
}
