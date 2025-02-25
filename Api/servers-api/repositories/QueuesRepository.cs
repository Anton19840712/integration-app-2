using MongoDB.Driver;
using servers_api.models.entities;

namespace servers_api.repositories
{
	public class QueuesRepository : MongoRepository<QueuesEntity>
	{
		public QueuesRepository(IMongoDatabase database, IConfiguration configuration)
			: base(database, configuration["MongoDbSettings:Collections:QueuesCollection"] ?? "queues_entities") { }
	}
}
