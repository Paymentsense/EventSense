using System;
using System.Threading.Tasks;
using EverStore.Contract;

namespace EverStore
{
    public interface IEventContext
    {
        Task<ResolvedEvent> AppendToStreamAsync(string stream, long expectedStreamVersion, Event @event);
        Task<ReadEvents> ReadStreamEventsForwardAsync(string stream, long start, int batchSize);
        Task<IDisposable> SubscribeToStreamAsync(string stream, long? lastCheckpoint, Func<CatchUpSubscription, ResolvedEvent, bool> eventAppeared, Action<CatchUpSubscription> liveProcessingStarted = null, Action<CatchUpSubscription, Exception> subscriptionDropped = null);
    }
}
