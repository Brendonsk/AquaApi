using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Services
{
    public class MensalService : CollectionService<Mensal>
    {
        public MensalService(MongoDbContext context) : base(context.Mensais) { }
    }
}
