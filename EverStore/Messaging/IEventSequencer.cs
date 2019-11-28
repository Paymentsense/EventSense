using EverStore.Domain;

namespace EverStore.Messaging
{
    internal interface IEventSequencer
    {
        void Initialise(long lastCheckpoint);
        EventSequence GetEventSequence(PersistedEvent @event, bool hasSubscribedToAllStream);
    }
}