namespace EverStore
{
    public class Event
    {
        string AggregateId { get; set; }
        string EventId { get; set; }
        private byte[] Data { get; set; } 
        long GlobalVersion { get; set; }
        long CategoryVersion { get; set; }
    }
}