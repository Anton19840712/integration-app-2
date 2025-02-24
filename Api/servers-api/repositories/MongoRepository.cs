using MongoDB.Driver;

namespace servers_api.repositories
{
	public class MongoRepository<T> where T : class
	{
		protected readonly IMongoCollection<T> _collection;

		public MongoRepository(IMongoDatabase database, string collectionName)
		{
			_collection = database.GetCollection<T>(collectionName);
		}

		public async Task InsertAsync(T entity)
		{
			await _collection.InsertOneAsync(entity);
		}

		public async Task<List<T>> GetAllAsync()
		{
			return await _collection.Find(_ => true).ToListAsync();
		}

		public async Task<T> GetByIdAsync(Guid id)
		{
			return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(Guid id, T entity)
		{
			await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);
		}

		public async Task DeleteAsync(Guid id)
		{
			await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
		}
	}
}
