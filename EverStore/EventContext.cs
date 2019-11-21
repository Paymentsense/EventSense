using System;
using System.Linq;
using System.Threading.Tasks;
using EverStore.Contract;
using EverStore.Domain;
using EverStore.Messaging;
using EverStore.Storage;
using Google.Cloud.PubSub.V1;
using MongoDB.Driver;
using OpenTracing;

namespace EverStore
{
    public class EventContext : IEventContext
    {
        private readonly IEventStreamSubscriber _eventStreamSubscriber;
        private readonly IEventStreamPublisher _eventStreamPublisher;
        private readonly IVersionRepository _versionRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventStreamSubscription _eventStreamSubscription;

        internal EventContext(IEventStreamSubscriber eventStreamSubscriber, IEventStreamPublisher eventStreamPublisher, IVersionRepository versionRepository, IEventRepository eventRepository, IEventStreamSubscription eventStreamSubscription)
        {
            _eventStreamSubscriber = eventStreamSubscriber;
            _eventStreamPublisher = eventStreamPublisher;
            _versionRepository = versionRepository;
            _eventRepository = eventRepository;
            _eventStreamSubscription = eventStreamSubscription;
        }

        public static IEventContext Create(string gcpProjectId, string eventStorageName, IMongoClient mongoClient, ITracer tracer = null)
        {
            var pubSubPublisherFactory = new PubSubPublisherFactory();
            var topicCreation = new TopicCreation(gcpProjectId);
            var conventionIdFactory = new ConventionIdFactory("", "", "");
            var eventPublisher = new EventStreamPublisher(topicCreation, tracer, pubSubPublisherFactory, conventionIdFactory);

            MongoContext.RegisterSerializerOptions();
            var mongoContext = MongoContext.CreateAsync(eventStorageName, mongoClient).GetAwaiter().GetResult();
            var versionRepository = new VersionRepository(mongoContext);
            var eventRepository = new EventRepository(mongoContext);

            var eventSequencer = new EventSequencer();
            var eventStreamSubscriber = new EventStreamSubscriber(new SubscriberClient.Settings(), eventSequencer, tracer);
            var subscriptionFactory = new SubscriptionCreation(gcpProjectId);
            var streamSubscriptionFactory = new EventStreamSubscription(topicCreation, subscriptionFactory, conventionIdFactory );

            return new EventContext(eventStreamSubscriber, eventPublisher, versionRepository, eventRepository, streamSubscriptionFactory);
        }

        public async Task<ResolvedEvent> AppendToStreamAsync(string stream, long expectedStreamVersion, Event @event)
        {
            if (@event == null)
            {
                throw new ArgumentException($"{nameof(@event)} is null", nameof(@event));
            }

            if (@event.Data == null || @event.Data.Length == 0)
            {
                throw new ArgumentException($"{nameof(@event)} is null", nameof(@event));
            }

            if (expectedStreamVersion < 0)
            {
                throw new ArgumentException($"{nameof(expectedStreamVersion)} cannot be below 0", nameof(expectedStreamVersion));
            }

            Stream.Parse(stream, out string streamAggregate, out string streamId);

            var nextGlobalVersion = _versionRepository.GetNextGlobalVersion();
            var persistedEvent = @event.ToModel(stream, expectedStreamVersion, nextGlobalVersion);

            _eventRepository.AppendEvent(@persistedEvent);

            await _eventStreamPublisher.Publish(persistedEvent, stream, streamAggregate, streamId);

            return persistedEvent.ToDto();
        }

        public async Task<ReadEvents> ReadStreamEventsForwardAsync(string stream, long start, int batchSize)
        {
            if (start < 0)
            {
                throw new ArgumentException($"{nameof(start)} cannot be below 0", nameof(start));
            }
            
            if (batchSize < 1)
            {
                throw new ArgumentException($"{nameof(batchSize)} cannot be below 1", nameof(batchSize));
            }

            if (string.IsNullOrWhiteSpace(stream))
            {
                throw new ArgumentException($"{nameof(stream)} cannot be empty", nameof(stream));
            }

            var persistedEvents = await _eventRepository.ReadEventsForwards(stream, start, batchSize);

            return new ReadEvents(persistedEvents);
        }

        public async Task SubscribeToStreamAsync(string stream, long? lastCheckpoint, Action<CatchUpSubscription, ResolvedEvent> eventAppeared, Action<CatchUpSubscription> liveProcessingStarted = null, Action<CatchUpSubscription, Exception> subscriptionDropped = null)
        {
            if (lastCheckpoint.HasValue && lastCheckpoint.Value < 0)
            {
                throw new ArgumentException($"{nameof(lastCheckpoint)} cannot be below 0", nameof(lastCheckpoint));
            }

            if (string.IsNullOrWhiteSpace(stream))
            {
                throw new ArgumentException($"{nameof(stream)} cannot be empty", nameof(stream));
            }

            if (eventAppeared == null)
            {
                throw new ArgumentException($"{nameof(eventAppeared)} cannot be null", nameof(eventAppeared));
            }

            string streamAggregate;
            var hasSubscribeToAllStream = string.Equals(stream, Stream.All, StringComparison.InvariantCulture);
            if (hasSubscribeToAllStream)
            {
                streamAggregate = Stream.All;
            }
            else
            {
                Stream.Parse(stream, out streamAggregate, out string _);
            }

            var subscription = await _eventStreamSubscription.CreateSubscriptionAsync(streamAggregate);

            var nextEventVersion = lastCheckpoint ?? 0;
            IAsyncCursor<PersistedEvent> eventCursor;
            if (hasSubscribeToAllStream)
            {
                eventCursor = await _eventRepository.ReadAllEventsForwards(nextEventVersion, 200);
            }
            else
            {
                eventCursor = await _eventRepository.ReadEventsForwards(stream, nextEventVersion, 200);
            }

            var catchUpSubscription = new CatchUpSubscription(stream, subscription.SubscriptionId);
            
            while (eventCursor.MoveNext())
            {
                foreach (var @event in eventCursor.Current)
                {
                    eventAppeared(catchUpSubscription, @event.ToDto());
                    nextEventVersion = hasSubscribeToAllStream ? @event.GlobalVersion : @event.StreamVersion;
                }
            }
            nextEventVersion++;
            await _eventStreamSubscriber.SubscribeAsync(stream, catchUpSubscription, nextEventVersion, subscription, eventAppeared, liveProcessingStarted, subscriptionDropped);
        }
    }
}
