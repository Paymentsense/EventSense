using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace EverStore
{
    internal class EventContext : IEventContext
    {
        private readonly IMongoContext _mongoContext;

        private EventContext(IMongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public static IEventContext Create(string eventStorageName, IMongoClient mongoClient)
        {
            var mongoContext = MongoContext.Create(eventStorageName, mongoClient);
            return new EventContext(mongoContext);
        }

        public Task<WrittenEvent> AppendToStreamAsync(string stream, long expectedVersion, Event @event)
        {
            throw new NotImplementedException();
        }

        public Task<ReadEvents> ReadStreamEventsForwardAsync(string stream, long start, int count)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToStreamFrom(string stream, long? lastCheckpoint, Action<CatchUpSubscription, ResolvedEvent> eventAppeared, Action<CatchUpSubscription> liveProcessingStarted = null,
            Action<CatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null)
        {
            //setup subscription on pubsub
            //read all data from mongodb

            //read from subscription, throw away data already read from mongo


            throw new NotImplementedException();
        }
    }
}
