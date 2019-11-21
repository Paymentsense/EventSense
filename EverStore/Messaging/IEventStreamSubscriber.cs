using System;
using System.Threading.Tasks;
using EverStore.Contract;

namespace EverStore.Messaging
{
    internal interface IEventStreamSubscriber
    {
        Task<DisposableSubscriber> SubscribeAsync(EventStreamSubscription subscription,
            Action<CatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<CatchUpSubscription> liveProcessingStarted = null,
            Action<CatchUpSubscription, Exception> subscriptionDropped = null);
    }
}