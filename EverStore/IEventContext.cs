using System;
using System.Threading.Tasks;

namespace EverStore
{
    public interface IEventContext
    {
        Task<WrittenEvent> AppendToStreamAsync(
            string stream,
            long expectedVersion,
            Event @event);

        Task<ReadEvents> ReadStreamEventsForwardAsync(
            string stream,
            long start,
            int count);

        void SubscribeToStreamFrom(
            string stream,
            long? lastCheckpoint,
            Action<CatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<CatchUpSubscription> liveProcessingStarted = null,
            Action<CatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null);
    }
}
