using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace servers_api.models.outbox;

public class OutboxMessage
{
	[BsonId]
	public ObjectId Id { get; set; }

	[BsonElement("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[BsonElement("processed_at")]
	public DateTime? ProcessedAt { get; set; } // Когда сообщение обработано

	[BsonElement("message")]
	public string Message { get; set; }

	[BsonElement("source")]
	public string Source { get; set; } // IP сервера

	[BsonElement("queue_in")]
	public string InQueueName { get; set; } // Название очереди, в которую будет публиковаться данное сообщение
											// после его сохранения в базу данных mongo db таблицу outbox
	[BsonElement("queue_out")]
	public string OutQueueName { get; set; } // Название очереди, в которую планируется, что будет публиковаться сообщение из bpm

	[BsonElement("routing_key")]
	public string RoutingKey { get; set; }

	[BsonElement("is_processed")]
	public bool IsProcessed { get; set; } = false;
}
