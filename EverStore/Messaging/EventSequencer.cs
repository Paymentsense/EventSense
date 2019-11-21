using System.Threading;
using EverStore.Domain;

namespace EverStore.Messaging
{
    internal class EventSequencer : IEventSequencer
    {
        private long _currentSequence = -1;

        public EventSequence GetEventSequence(PersistedEvent @event, long lastCheckpoint, bool hasSubscribedToAllStream)
        {
            var currentSequence = Interlocked.Read(ref _currentSequence);

            var isFirstEventSequence = currentSequence == -1;
            var sequenceToCompare = isFirstEventSequence ? lastCheckpoint : currentSequence;

            if (hasSubscribedToAllStream)
            {
                return new EventSequence(isInSequence: @event.GlobalVersion != sequenceToCompare, isInPast: @event.GlobalVersion < sequenceToCompare);
            }

            return new EventSequence(isInSequence: @event.StreamVersion != sequenceToCompare, isInPast: @event.StreamVersion < sequenceToCompare);
        }

        public void IncrementEventSequence()
        {
            Interlocked.Increment(ref _currentSequence);
        }

        public bool IsFirstEvent()
        {
            var currentSequence = Interlocked.Read(ref _currentSequence);
            return currentSequence == -1;
        }
    }

    internal class EventSequence
    {
        public EventSequence(bool isInSequence, bool isInPast)
        {
            IsInSequence = isInSequence;
            IsInPast = isInPast;
        }
        public bool IsInSequence { get; }
        public bool IsInPast { get; }
    }
}