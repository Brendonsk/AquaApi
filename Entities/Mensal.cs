using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;

namespace MqttApiPg.Entities
{
    public class Mensal: BaseEntity
    {
        public int Mes { get; set; }
        public int Ano { get; set; }

        public decimal ConsumoTotal { get; set; }
    }
}
