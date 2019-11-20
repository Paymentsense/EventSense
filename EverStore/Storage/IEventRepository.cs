using EverStore.Domain;

namespace EverStore.Storage
{
    internal interface IEventRepository
    {
        void AppendEvent(PersistedEvent @event);
    }
}