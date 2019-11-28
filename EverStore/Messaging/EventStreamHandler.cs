using System;
using EverStore.Contract;
using EverStore.Domain;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal class EventStreamHandler : IEventStreamHandler
    {
        private readonly IEventSequencer _eventSequencer;

        public EventStreamHandler(IEventSequencer eventSequencer)
        {
            _eventSequencer = eventSequencer;
        }

        public SubscriberClient.Reply Handle(PersistedEvent @event, EventStreamSubscription subscription, Action<CatchUpSubscription, ResolvedEvent> eventAppeared, Action<CatchUpSubscription> liveProcessingStarted = null)
        {
            var eventSequence = _eventSequencer.GetEventSequence(@event, subscription.HasSubscribeToAllStream);
            if (eventSequence.IsInPast)
            {
                return SubscriberClient.Reply.Ack;
            }

            if (eventSequence.IsFirstEvent)
            {
                liveProcessingStarted?.Invoke(subscription.CatchUpSubscription);
            }

            eventAppeared(subscription.CatchUpSubscription, @event.ToDto());

            return SubscriberClient.Reply.Ack;
        }
    }
}