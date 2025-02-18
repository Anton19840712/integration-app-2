using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class IntegrationEntity
{
	[BsonId]
	[BsonRepresentation(BsonType.String)] 
	public Guid Id { get; set; }

	public string InQueueName { get; set; }
	public string OutQueueName { get; set; }

	[BsonElement("incomingModel")]
	[BsonRepresentation(BsonType.String)]
	public string IncomingModel { get; set; }
}
