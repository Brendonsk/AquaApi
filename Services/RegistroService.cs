using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Services
{
    public class RegistroService : CollectionService<Registro>
    {
        public RegistroService(MongoDbContext context) : base(context.Registros) { }

        public async Task<List<Registro>> GetAllFalseRegistros()
        {
            return await
                (await collection.FindAsync(x => !x.DataSolucao.HasValue)).ToListAsync();
        }

        public async Task DeleteRegistrosByMessage(string message)
        {
           await collection.DeleteManyAsync(x => x.Mensagem.Equals(message));
        }
    }
}
