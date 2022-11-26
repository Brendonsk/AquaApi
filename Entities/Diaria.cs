using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MqttApiPg.Entities
{
    public class Diaria: BaseEntity
    {
        public decimal Valor { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime DiaHora { get; set; }
    }
}
