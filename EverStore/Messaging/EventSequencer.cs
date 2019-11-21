using System.Threading;
using EverStore.Domain;

namespace EverStore.Messaging
{
    internal class EventSequencer : IEventSequencer
    {
        private const long StartingSequence = -1;
        private long _currentSequence = StartingSequence;

        public void Initialise(long lastCheckpoint)
        {
            Interlocked.CompareExchange(ref _currentSequence, lastCheckpoint, StartingSequence);
        }

        public EventSequence GetEventSequence(PersistedEvent @event, bool hasSubscribedToAllStream)
        {
            var currentSequence = Interlocked.Read(ref _currentSequence);
            
            if (hasSubscribedToAllStream)
            {
                return new EventSequence(
                    isInSequence:@event.GlobalVersion == currentSequence,
                    isInPast:@event.GlobalVersion < currentSequence, 
                    currentSequence == StartingSequence);
            }

            return new EventSequence(
                isInSequence: @event.StreamVersion == currentSequence,
                isInPast: @event.StreamVersion < currentSequence,
                currentSequence == StartingSequence);
        }

        public void IncrementEventSequence()
        {
            Interlocked.Increment(ref _currentSequence);
        }
    }
}