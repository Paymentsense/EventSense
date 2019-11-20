using System;
using System.Threading.Tasks;
using EverStore.Messaging;
using MongoDB.Driver;
using OpenTracing;

namespace EverStore
{
    public class EventContext : IEventContext
    {
        private readonly IMongoContext _mongoContext;
        private readonly IEventStreamPublisher _eventStreamPublisher;

        private EventContext(IMongoContext mongoContext, IEventStreamPublisher eventStreamPublisher)
        {
            _mongoContext = mongoContext;
            _eventStreamPublisher = eventStreamPublisher;
        }

        public static IEventContext Create(string gcpProjectId, string eventStorageName, IMongoClient mongoClient, ITracer tracer = null)
        {
            var pubSubPublisherFactory = new PubSubPublisherFactory();
            var publisher = new EventStreamPublisher(new TopicFactory(gcpProjectId), tracer, pubSubPublisherFactory);
            var mongoContext = MongoContext.Create(eventStorageName, mongoClient);
            return new EventContext(mongoContext, publisher);
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
