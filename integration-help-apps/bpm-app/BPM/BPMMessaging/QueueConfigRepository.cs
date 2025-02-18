using MongoDB.Driver;

namespace BPMMessaging
{
	public class QueueConfigRepository
	{
		private readonly IMongoCollection<QueueConfig> _collection;

		public QueueConfigRepository(IMongoDatabase database)
		{
			_collection = database.GetCollection<QueueConfig>("queue_configs");
		}

		public async Task<List<QueueConfig>> GetActiveQueuesAsync()
		{
			return await _collection.Find(q => q.IsActive).ToListAsync();
		}

		public async Task AddQueueConfigAsync(QueueConfig config)
		{
			await _collection.InsertOneAsync(config);
		}
	}
}
