using MongoDB.Bson;
using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Services
{
    public class DiariaService : CollectionService<Diaria>
    {
        public DiariaService(MongoDbContext context) : base(context.Diarias) { }

        public async Task<List<Diaria>> GetDiariasByMonthOfYear(int ano, int mes)
        {
            var startOfMonth = new DateTime(ano, mes, 1, 0, 0, 0);
            var startOfNextMonth = startOfMonth.AddMonths(1);
            var bldr = Builders<Diaria>.Filter;
            var expenseMonthFilter = bldr.And(
                bldr.Gte(x => x.DiaHora, startOfMonth),
                bldr.Lt(x => x.DiaHora, startOfNextMonth));

            return await
                (await collection.FindAsync(expenseMonthFilter)).ToListAsync();

        }

        public async Task<Diaria?> GetUltimaDiariaDoMes(int ano, int mes)
        {
            return (await GetDiariasByMonthOfYear(ano, mes))
                .OrderBy(x => x.DiaHora)
                .FirstOrDefault();
        }
    }
}
