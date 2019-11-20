using EverStore.Domain;

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
    }
}
