using System;
using System.Threading.Tasks;
using EverStore.Contract;
using EverStore.Domain;
using EverStore.Messaging;
using EverStore.Storage;
using MongoDB.Driver;
using OpenTracing;

namespace EverStore
{
    public class EventContext : IEventContext
    {
        private readonly IEventStreamPublisher _eventStreamPublisher;
        private readonly IVersionRepository _versionRepository;
        private readonly IEventRepository _eventRepository;

        internal EventContext(IEventStreamPublisher eventStreamPublisher, IVersionRepository versionRepository, IEventRepository eventRepository)
        {
            _eventStreamPublisher = eventStreamPublisher;
            _versionRepository = versionRepository;
            _eventRepository = eventRepository;
        }

        public static IEventContext Create(string gcpProjectId, string eventStorageName, IMongoClient mongoClient, ITracer tracer = null)
        {
            var pubSubPublisherFactory = new PubSubPublisherFactory();
            var eventPublisher = new EventStreamPublisher(new TopicFactory(gcpProjectId), tracer, pubSubPublisherFactory);
            MongoContext.RegisterSerializerOptions();
            var mongoContext = MongoContext.CreateAsync(eventStorageName, mongoClient).GetAwaiter().GetResult();
            var versionRepository = new VersionRepository(mongoContext);
            var eventRepository = new EventRepository(mongoContext);
            return new EventContext(eventPublisher, versionRepository, eventRepository);
        }

        public async Task<ResolvedEvent> AppendToStreamAsync(string stream, long expectedStreamVersion, Event @event)
        {
            if (@event == null)
            {
                throw new ArgumentException("Event is null", nameof(@event));
            }

            if (@event.Data == null || @event.Data.Length == 0)
            {
                throw new ArgumentException("Event is null", nameof(@event));
            }

            if (expectedStreamVersion < 0)
            {
                throw new ArgumentException("ExpectedStreamVersion cannot be below 0", nameof(@event));
            }

            Stream.Parse(stream, out string streamAggregate, out string streamId);

            var nextGlobalVersion = _versionRepository.GetNextGlobalVersion();
            var persistedEvent = @event.ToModel(stream, expectedStreamVersion, nextGlobalVersion);

            _eventRepository.AppendEvent(@persistedEvent);

            await _eventStreamPublisher.Publish(persistedEvent, stream, streamAggregate, streamId);

            return persistedEvent.ToDto();
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
