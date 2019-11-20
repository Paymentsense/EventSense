using MongoDB.Driver;

namespace EverStore.Storage
{
    internal interface IMongoContext
    {
        IMongoCollection<T> Collection<T>();
    }
}