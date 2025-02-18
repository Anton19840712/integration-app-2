using MongoDB.Bson;
using MongoDB.Driver;
using servers_api.models.outbox;

namespace servers_api.repositories;

public class MongoOutboxRepository : IOutboxRepository
{
	private readonly IMongoCollection<OutboxMessage> _collection;

	public MongoOutboxRepository(IMongoDatabase database)
	{
		_collection = database.GetCollection<OutboxMessage>("outbox_messages");
	}

	/// <summary>
	/// Сохраняем полученное сообщение в MongoDB
	/// </summary>
	public async Task SaveMessageAsync(OutboxMessage message)
	{
		await _collection.InsertOneAsync(message);
	}

	/// <summary>
	/// Получает непросессированные сообщения
	/// </summary>
	public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync()
	{
		return await _collection.Find(m => !m.IsProcessed).ToListAsync();
	}

	/// <summary>
	/// Помечает сообщение как обработанное
	/// </summary>
	public async Task MarkMessageAsProcessedAsync(ObjectId messageId)
	{
		var update = Builders<OutboxMessage>.Update
			.Set(m => m.IsProcessed, true)
			.Set(m => m.ProcessedAt, DateTime.UtcNow);
		await _collection.UpdateOneAsync(m => m.Id == messageId, update);
	}

	/// <summary>
	/// Удаляет сообщения, которые старше указанного времени
	/// </summary>
	public async Task<int> DeleteOldMessagesAsync(TimeSpan olderThan)
	{
		var filter = Builders<OutboxMessage>.Filter.Lt(m => m.CreatedAt, DateTime.UtcNow - olderThan);
		var result = await _collection.DeleteManyAsync(filter);
		return (int)result.DeletedCount;
	}
}
