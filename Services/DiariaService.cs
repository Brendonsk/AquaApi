using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Services
{
    public class DiariaService : CollectionService<Diaria>
    {
        public DiariaService(MongoDbContext context) : base(context.Diarias) { }
    }
}
