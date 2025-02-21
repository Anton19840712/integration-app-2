using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BPMMessaging.models
{
	public class IncidentEntity
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		public string InQueueName { get; set; }
		public string OutQueueName { get; set; }
		public string IncidentData { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	}
}
