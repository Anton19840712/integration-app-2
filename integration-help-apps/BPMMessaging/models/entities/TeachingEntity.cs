using MongoDB.Bson;

namespace BPMMessaging.models.entities
{
	public class TeachingEntity : AuditableEntity
	{
		public string InQueueName { get; set; }
		public string OutQueueName { get; set; }
		public BsonDocument IncomingModel { get; set; }
	}
}
