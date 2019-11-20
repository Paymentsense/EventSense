using EverStore.Contract;

namespace EverStore.Domain
{
    internal static class PersistedEventTranslator
    {
        public static PersistedEvent ToModel(this Event @event, string stream, long expectedStreamVersion, long globalVersion)
        {
            return new PersistedEvent
            {
                GlobalVersion = globalVersion,
                Stream = stream,
                StreamVersion = expectedStreamVersion,
                Data = @event.Data
            };
        }

        public static ResolvedEvent ToDto(this PersistedEvent @event)
        {
            return new ResolvedEvent
            {
                Data = @event.Data,
                GlobalVersion = @event.GlobalVersion,
                Stream = @event.Stream,
                StreamVersion = @event.StreamVersion
            };
        }
    }
}