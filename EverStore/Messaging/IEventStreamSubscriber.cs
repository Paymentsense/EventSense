using System;
using System.Threading.Tasks;
using EverStore.Contract;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface IEventStreamSubscriber
    {
        Task SubscribeAsync(string subscribedStream,
            CatchUpSubscription catchUpSubscription,
            long? lastCheckpoint,
            SubscriptionName streamSubscription,
            Action<CatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<CatchUpSubscription> liveProcessingStarted = null,
            Action<CatchUpSubscription, Exception> subscriptionDropped = null);
    }
}