using EverStore.Domain;

namespace EverStore.Messaging
{
    internal interface IEventSequencer
    {
        EventSequence GetEventSequence(PersistedEvent @event, long? lastCheckpoint, bool hasSubscribedToAllStream);
        void IncrementEventSequence();
        bool IsFirstEvent();
    }
}