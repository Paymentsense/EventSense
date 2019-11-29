using EverStore.Domain;

namespace EverStore.Messaging
{
    internal class EventSequencer : IEventSequencer
    {
        private long _initialVersion;

        public void Initialise(long lastCheckpoint)
        {
            _initialVersion = lastCheckpoint;
        }

        public EventSequence GetEventSequence(PersistedEvent @event, bool hasSubscribedToAllStream)
        {
            if (hasSubscribedToAllStream)
            {
                return new EventSequence(
                    isInPast:@event.GlobalVersion < _initialVersion,
                    _initialVersion == @event.GlobalVersion);
            }

            return new EventSequence(
                isInPast: @event.StreamVersion < _initialVersion,
                _initialVersion == @event.StreamVersion);
        }
    }
}