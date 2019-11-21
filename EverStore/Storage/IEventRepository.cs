using System.Threading.Tasks;
using EverStore.Domain;
using MongoDB.Driver;

namespace EverStore.Storage
{
    internal interface IEventRepository
    {
        void AppendEvent(PersistedEvent @event);
        Task<IAsyncCursor<PersistedEvent>> ReadEventsForwards(string stream, long start, int batchSize);
        Task<IAsyncCursor<PersistedEvent>> ReadAllEventsForwards(long start, int batchSize);
    }
}