using MongoDB.Driver;
using servers_api.models.entities;

namespace servers_api.repositories
{
	public class QueuesRepository : MongoRepository<QueuesEntity>
	{
		public QueuesRepository(IMongoDatabase database) : base(database, "queues_entities") { }
	}
}
