using System.Threading.Tasks;
using EverStore.Domain;
using MongoDB.Driver;

namespace EverStore.Storage
{
    internal class EventRepository: IEventRepository
    {
        private readonly IMongoContext _mongoContext;

        public EventRepository(IMongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public void AppendEvent(PersistedEvent @event)
        {
            _mongoContext.Collection<PersistedEvent>().InsertOne(@event);
        }

        public async Task<IAsyncCursor<PersistedEvent>> ReadEventsForwards(string stream, long start, int batchSize)
        {
            var filter = Builders<PersistedEvent>.Filter.And(Builders<PersistedEvent>.Filter.Eq(e => e.Stream, stream), Builders<PersistedEvent>.Filter.Gte(e => e.StreamVersion, start));
            var sort = Builders<PersistedEvent>.Sort.Ascending(e => e.StreamVersion);
            return await _mongoContext.Collection<PersistedEvent>().FindAsync(filter, new FindOptions<PersistedEvent> {BatchSize = batchSize, Sort = sort});
        }
        
        public async Task<IAsyncCursor<PersistedEvent>> ReadAllEventsForwards(long start, int batchSize)
        {
            var filter = Builders<PersistedEvent>.Filter.Gte(e => e.GlobalVersion, start);
            var sort = Builders<PersistedEvent>.Sort.Ascending(e => e.GlobalVersion);
            return await _mongoContext.Collection<PersistedEvent>().FindAsync(filter, new FindOptions<PersistedEvent> {BatchSize = batchSize, Sort = sort});
        }
    }
}
