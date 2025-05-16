
using MongoDB.Bson.Serialization.Attributes;

namespace CommonGateLib.Entities
{
    [BsonIgnoreExtraElements]
    public class IncidentEntity : AuditableEntity
    {
        public string Payload { get; set; }
    }
}
