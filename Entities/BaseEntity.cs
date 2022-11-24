using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;

namespace MqttApiPg.Entities
{
    public abstract class BaseEntity
    {
        [SwaggerSchema(ReadOnly = true)]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
    }
}
