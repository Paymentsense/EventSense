using EverStore.Contract;
using Google.Cloud.PubSub.V1;

namespace EverStore
{
    internal class EventStreamSubscription
    {
        public string Stream { get; }
        public CatchUpSubscription CatchUpSubscription { get; }
        public long NextEventVersion { get; }
        public SubscriptionName SubscriptionName { get; }
        public bool HasSubscribeToAllStream { get; }

        public EventStreamSubscription(string stream, 
            CatchUpSubscription catchUpSubscription, 
            long nextEventVersion, 
            SubscriptionName subscriptionName, 
            bool hasSubscribeToAllStream)
        {
            Stream = stream;
            CatchUpSubscription = catchUpSubscription;
            NextEventVersion = nextEventVersion;
            SubscriptionName = subscriptionName;
            HasSubscribeToAllStream = hasSubscribeToAllStream;
        }
    }
}