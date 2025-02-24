using MongoDB.Bson;
using MongoDB.Driver;
using servers_api.models.outbox;

namespace servers_api.repositories
{
	public class OutboxRepository : MongoRepository<OutboxMessage>, IOutboxRepository
	{
		public OutboxRepository(IMongoDatabase database) : base(database, "outbox_messages") { }

		public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync()
		{
			return await _collection.Find(m => !m.IsProcessed).ToListAsync();
		}

		public async Task MarkMessageAsProcessedAsync(ObjectId messageId)
		{
			var update = Builders<OutboxMessage>.Update
				.Set(m => m.IsProcessed, true)
				.Set(m => m.ProcessedAt, DateTime.UtcNow);
			await _collection.UpdateOneAsync(m => m.Id == messageId, update);
		}

		public async Task<int> DeleteOldMessagesAsync(TimeSpan olderThan)
		{
			var filter = Builders<OutboxMessage>.Filter.And(
				Builders<OutboxMessage>.Filter.Lt(m => m.CreatedAt, DateTime.UtcNow - olderThan),
				Builders<OutboxMessage>.Filter.Eq(m => m.IsProcessed, true)
			);

			var result = await _collection.DeleteManyAsync(filter);
			return (int)result.DeletedCount;
		}

		public async Task SaveMessageAsync(OutboxMessage message)
		{
			await _collection.InsertOneAsync(message);
		}
	}
}
