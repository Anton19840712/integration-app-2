using BPMMessaging.enums;
using BPMMessaging.models.entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;

namespace BPMMessaging.models.dtos
{
	public class OutboxMessage
	{
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public Guid Id { get; set; } = Guid.NewGuid();

		[BsonElement("eventType")]
		public EventTypes EventType { get; set; }

		[BsonElement("isProcessed")]
		public bool IsProcessed { get; set; }

		[BsonElement("outQueue")]
		public string OutQueue { get; set; }

		[BsonElement("inQueue")]
		public string InQueue { get; set; }

		[BsonElement("payload")]
		public BsonDocument Payload { get; set; }

		[BsonElement("createdAt")]
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
