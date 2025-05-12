using MongoDB.Bson.Serialization.Attributes;

namespace test_mongo_api_app.models
{
	[BsonIgnoreExtraElements]
	public class QueuesEntity : AuditableEntity
	{
		[BsonElement("inQueueName")]
		public string InQueueName { get; set; }

		[BsonElement("outQueueName")]
		public string OutQueueName { get; set; }
	}
}
