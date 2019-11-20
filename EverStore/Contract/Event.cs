using System;

namespace EverStore.Contract
{
    public class Event
    {
        public byte[] Data { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}