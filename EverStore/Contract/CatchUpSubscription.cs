namespace EverStore.Contract
{
    public class CatchUpSubscription
    {
        public CatchUpSubscription(string subscribedStream, string subscriptionId)
        {
            SubscribedStream = subscribedStream;
            SubscriptionId = subscriptionId;
        }

        public string SubscribedStream { get; }

        public string SubscriptionId { get; }
    }
}