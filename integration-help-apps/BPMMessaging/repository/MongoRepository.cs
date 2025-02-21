using BPMMessaging.models.dtos;
using BPMMessaging.models.entities;
using BPMMessaging.models.settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BPMMessaging.repository
{
	public class MongoRepository<T> : IMongoRepository<T> where T : class
	{
		private readonly IMongoCollection<T> _collection;

		public MongoRepository(
			IMongoClient mongoClient,
			IOptions<MongoDbSettings> settings)
		{
			var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
			string collectionName = GetCollectionName(typeof(T), settings.Value);
			_collection = database.GetCollection<T>(collectionName);
		}

		private string GetCollectionName(Type entityType, MongoDbSettings settings)
		{
			return entityType.Name switch
			{
				nameof(IncidentEntity) => settings.Collections.IncidentCollection,
				nameof(TeachingEntity) => settings.Collections.TeachingCollection,
				nameof(OutboxMessage) => settings.Collections.OutboxMessages,
				_ => throw new ArgumentException($"Неизвестный тип {entityType.Name}")
			};
		}

		public async Task<T> GetByIdAsync(string id) =>
			await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();

		public async Task<IEnumerable<T>> GetAllAsync() =>
			await _collection.Find(_ => true).ToListAsync();

		public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter) =>
			await _collection.Find(filter).ToListAsync();

		public async Task InsertAsync(T entity) =>
			await _collection.InsertOneAsync(entity);

		public async Task UpdateAsync(string id, T entity) =>
			await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);

		public async Task DeleteAsync(string id) =>
			await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
	}
}
