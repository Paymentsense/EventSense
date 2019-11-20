namespace EverStore
{
    public class ResolvedEvent : Event
    {
        public string Stream { get; set; }
        public long StreamVersion { get; set; }
        public long GlobalVersion { get; set; }
    }
}