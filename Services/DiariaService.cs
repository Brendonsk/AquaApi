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
            //PipelineDefinition<Diaria, BsonDocument> pipeline = new BsonDocument[]
            //{
            //    new BsonDocument("$project", new BsonDocument()
            //            .Add("mes", new BsonDocument()
            //                    .Add("$month", "$DiaHora")
            //            )
            //            .Add("ano", new BsonDocument()
            //                    .Add("$year", "$DiaHora")
            //            )
            //            .Add("root", "$$ROOT")),
            //    new BsonDocument("$match", new BsonDocument()
            //            .Add("mes", mes)),
            //    new BsonDocument("$match", new BsonDocument()
            //            .Add("ano", ano))
            //};

            var startOfMonth = new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Local);
            var startOfNextMonth = startOfMonth.AddMonths(1);
            var bldr = Builders<Diaria>.Filter;
            var expenseMonthFilter = bldr.And(
                bldr.Gte(x => x.DiaHora, startOfMonth),
                bldr.Lt(x => x.DiaHora, startOfNextMonth));

            return await
                (await collection.FindAsync(expenseMonthFilter)).ToListAsync();

        }
    }
}
