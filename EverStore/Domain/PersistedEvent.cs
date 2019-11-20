using System;
using MongoDB.Bson.Serialization.Attributes;

namespace EverStore.Domain
{
    internal class PersistedEvent
    {
        [BsonId]
        public long GlobalVersion { get; set; }

        public string Stream { get; set; }
        public long StreamVersion { get; set; }

        public byte[] Data { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
