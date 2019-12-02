using System;
using EverStore.Contract;
using EverStore.Domain;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface IEventStreamHandler
    {
        SubscriberClient.Reply Handle(PersistedEvent @event, EventStreamSubscription subscription, Func<CatchUpSubscription, ResolvedEvent, bool> eventAppeared, Action<CatchUpSubscription> liveProcessingStarted = null);
    }
}