using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BPMMessaging.models.entities
{
	public abstract class AuditableEntity
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAtUtc { get; set; }
		public DateTime? DeletedAtUtc { get; set; }

		public string CreatedBy { get; set; }
		public string UpdatedBy { get; set; }
		public string DeletedBy { get; set; }

		public bool IsDeleted { get; set; } = false;

		public int Version { get; set; } = 1;

		public string IpAddress { get; set; }
		public string UserAgent { get; set; }
		public string CorrelationId { get; set; }
	}
}
