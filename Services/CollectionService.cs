using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Services
{
    public abstract class CollectionService<T>
        where T : BaseEntity
    {
        protected readonly IMongoCollection<T> collection;

        protected CollectionService(IMongoCollection<T> collection)
        {
            this.collection = collection;
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await (await collection.FindAsync(x => x.Id!.Equals(id)))
                    .SingleOrDefaultAsync();
        }

        public async Task<List<T>> GetAsync()
        {
            return await
                (await collection.FindAsync(_ => true))
                    .ToListAsync();
        }

        public async Task CreateAsync(T entity)
        {
            await collection.InsertOneAsync(entity);
        }

        public async Task<T?> UpdateAsync(string id, T entity)
        {
            if (await EntityExists(id))
            {
                entity.Id = id;
                await collection.ReplaceOneAsync(x => x.Id == id, entity);
                return entity;
            }

            return null;
        }

        public async Task<T?> DeleteAsync(string id)
        {
            T? entity = await GetByIdAsync(id);
            if (entity is not null)
            {
                var res = await collection.DeleteOneAsync(x => x.Id == id);
                return entity;
            }

            return null;
        }

        public async Task<bool> EntityExists(string id)
        {
            return (await collection.FindAsync(x => x.Id == id)).Any();
        }
    }
}
