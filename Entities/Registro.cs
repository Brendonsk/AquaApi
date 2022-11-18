using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace MqttApiPg.Entities
{
    public class Registro
    {
        [SwaggerSchema(ReadOnly = true)]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime DataOcorrencia { get; set; }

        public DateTime DataSolucao { get; set; }

        public string Mensagem { get; set; }

        public bool Decisao { get; set; }
    }
}
