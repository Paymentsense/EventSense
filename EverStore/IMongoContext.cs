using MongoDB.Driver;

namespace EverStore
{
    internal interface IMongoContext
    {
        IMongoCollection<T> Collection<T>();
    }
}