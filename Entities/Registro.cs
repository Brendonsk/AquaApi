using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace MqttApiPg.Entities
{
    public class Registro: BaseEntity
    {
        public DateTime DataOcorrencia { get; set; }

        public DateTime? DataSolucao { get; set; }

        public string Mensagem { get; set; }

        public bool Decisao { get; set; }
    }
}
