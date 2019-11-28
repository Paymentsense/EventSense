using EverStore.Domain;

namespace EverStore.Messaging
{
    internal class EventSequencer : IEventSequencer
    {
        private long _currentSequence;

        public void Initialise(long lastCheckpoint)
        {
            _currentSequence = lastCheckpoint;
        }

        public EventSequence GetEventSequence(PersistedEvent @event, bool hasSubscribedToAllStream)
        {
            if (hasSubscribedToAllStream)
            {
                return new EventSequence(
                    isInPast:@event.GlobalVersion < _currentSequence,
                    _currentSequence == @event.GlobalVersion);
            }

            return new EventSequence(
                isInPast: @event.StreamVersion < _currentSequence,
                _currentSequence == @event.StreamVersion);
        }
    }
}