using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MqttApiPg.Entities;
using MqttApiPg.Models;

namespace MqttApiPg
{
    public class MongoDbContext
    {
        private IMongoDatabase _database { get; }

        public MongoDbContext(IOptions<AquaDatabaseSettings> dbSettings)
        {
            var clientSettings = MongoClientSettings.FromConnectionString(dbSettings.Value.ConnectionString);
            
            //Configurar opções adicionais do banco aqui

            var mongoClient = new MongoClient(clientSettings);
            _database = mongoClient.GetDatabase("Aqua");
        }

        public IMongoCollection<Registro> Registros => _database.GetCollection<Registro>("registro");
        public IMongoCollection<Diaria> Diarias => _database.GetCollection<Diaria>("diaria");
        public IMongoCollection<Mensal> Mensais => _database.GetCollection<Mensal>("mensal");

    }
}
