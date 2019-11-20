namespace EverStore.Contract
{
    public class ReadEvents
    {
        public ResolvedEvent[] Events { get; set; }
        public bool IsEndOfStream { get; set; }
        public long NextEventNumber { get; set; }
    }
}