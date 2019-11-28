namespace EverStore.Messaging
{
    internal class EventSequence
    {
        public EventSequence(bool isInPast, bool isFirstEvent)
        {
            IsInPast = isInPast;
            IsFirstEvent = isFirstEvent;
        }

        public bool IsInPast { get; }
        public bool IsFirstEvent { get; }

    }
}