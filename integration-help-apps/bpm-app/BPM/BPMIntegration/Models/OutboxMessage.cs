using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BPMIntegration.Models
{
	public class OutboxMessage
	{
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public Guid Id { get; set; } = Guid.NewGuid();

		[BsonElement("eventType")]
		public EventTypes EventType { get; set; }

		[BsonElement("payload")]
		public IntegrationEntity Payload { get; set; }

		[BsonElement("isProcessed")]
		public bool IsProcessed { get; set; }

		[BsonElement("outQueue")]
		public string OutQueue { get; set; }

		[BsonElement("createdAt")]
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
