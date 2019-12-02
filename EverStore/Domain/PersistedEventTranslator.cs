using System;
using EverStore.Contract;
using EverStore.Messaging;
using Google.Cloud.PubSub.V1;

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
                Data = @event.Data,
                CreatedAt = @event.CreatedAt
            };
        }

        public static PersistedEvent ToModel(this PubsubMessage message)
        {
            return new PersistedEvent
            {
                GlobalVersion = long.Parse(message.Attributes[EventStreamAttributes.GlobalVersion]),
                Stream = message.Attributes[EventStreamAttributes.Stream],
                StreamId = message.Attributes[EventStreamAttributes.StreamId],
                StreamVersion = long.Parse(message.Attributes[EventStreamAttributes.StreamVersion]),
                Data = message.Data.ToByteArray(),
                CreatedAt = DateTimeOffset.Parse(message.Attributes[EventStreamAttributes.CreatedAt]),
            };
        }

        public static ResolvedEvent ToDto(this PersistedEvent @event)
        {
            return new ResolvedEvent
            {
                Data = @event.Data,
                GlobalVersion = @event.GlobalVersion,
                Stream = @event.Stream,
                StreamVersion = @event.StreamVersion,
                CreatedAt = @event.CreatedAt
            };
        }
    }
}