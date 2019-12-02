using System;
using System.Threading.Tasks;
using EverStore.Contract;

namespace EverStore.Messaging
{
    internal interface IEventStreamSubscriber
    {
        Task<DisposableSubscriber> SubscribeAsync(EventStreamSubscription subscription,
            Func<CatchUpSubscription, ResolvedEvent, bool> eventAppeared,
            Action<CatchUpSubscription> liveProcessingStarted = null,
            Action<CatchUpSubscription, Exception> subscriptionDropped = null);
    }
}