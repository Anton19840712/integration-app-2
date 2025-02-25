using System.Linq.Expressions;
using MongoDB.Driver;

namespace servers_api.repositories
{
	public class MongoRepository<T> where T : class
	{
		protected readonly IMongoCollection<T> _collection;

		public MongoRepository(IMongoDatabase database, string collectionName)
		{

			if (string.IsNullOrWhiteSpace(collectionName))
			{
				throw new ArgumentException("Имя коллекции не может быть пустым", nameof(collectionName));
			}

			_collection = database.GetCollection<T>(collectionName)
						 ?? throw new InvalidOperationException($"Коллекция {collectionName} не найдена.");
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

		public async Task UpdateAsync(Guid id, T updatedEntity)
		{
			var filter = Builders<T>.Filter.Eq("_id", id);
			var existingEntity = await _collection.Find(filter).FirstOrDefaultAsync();

			if (existingEntity == null)
			{
				throw new InvalidOperationException($"Документ с ID {id} не найден");
			}

			var updateDefinitionBuilder = Builders<T>.Update;
			var updates = new List<UpdateDefinition<T>>();

			// Перебираем все свойства модели
			foreach (var property in typeof(T).GetProperties())
			{
				if (property.Name == "Version") continue; // Пропускаем Version, т.к. он изменяется отдельно

				var oldValue = property.GetValue(existingEntity);
				var newValue = property.GetValue(updatedEntity);

				if (newValue != null && !newValue.Equals(oldValue))
				{
					updates.Add(updateDefinitionBuilder.Set(property.Name, newValue));
				}
			}

			// Добавляем обновление времени
			updates.Add(updateDefinitionBuilder.Set("UpdatedAtUtc", DateTime.UtcNow));
			updates.Add(updateDefinitionBuilder.Inc("Version", 1));

			if (updates.Count > 0)
			{
				var updateDefinition = updateDefinitionBuilder.Combine(updates);
				await _collection.UpdateOneAsync(filter, updateDefinition);
			}
		}

		public async Task DeleteAsync(Guid id)
		{
			await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
		}

		public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter) =>
			await _collection.Find(filter).ToListAsync();
	}
}
