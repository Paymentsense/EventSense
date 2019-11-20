namespace EverStore
{
    public class WrittenEvent
    {
        public SliceReadStatus Status { get; private set; }
        public ResolvedEvent Event { get; private set; }
    }
}