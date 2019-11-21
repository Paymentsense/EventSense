namespace EverStore.Messaging
{
    internal class EventSequence
    {
        public EventSequence(bool isInSequence, bool isInPast, bool isFirstEvent)
        {
            IsInSequence = isInSequence;
            IsInPast = isInPast;
            IsFirstEvent = isFirstEvent;
        }

        public bool IsInSequence { get; }
        public bool IsInPast { get; }
        public bool IsFirstEvent { get; set; }

    }
}