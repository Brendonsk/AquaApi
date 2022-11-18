using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;

namespace MqttApiPg.Entities
{
    public class Mensal
    {
        [SwaggerSchema(ReadOnly = true)]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int Mes { get; set; }
        public int Ano { get; set; }

        public decimal ConsumoTotal { get; set; }
    }
}
