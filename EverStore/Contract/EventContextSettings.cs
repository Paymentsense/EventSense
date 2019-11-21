namespace EverStore.Contract
{
    public class EventContextSettings
    {
        public EventContextSettings(string gcpProjectId, string eventStorageName, string subscriptionIdentifier, string subscriptionTopicPrefix, string subscriptionTopicPostfix)
        {
            GcpProjectId = gcpProjectId;
            EventStorageName = eventStorageName;
            SubscriptionIdentifier = subscriptionIdentifier;
            SubscriptionTopicPrefix = subscriptionTopicPrefix;
            SubscriptionTopicPostfix = subscriptionTopicPostfix;
        }

        public string GcpProjectId { get; }
        public string EventStorageName { get; }
        public string SubscriptionIdentifier { get; }
        public string SubscriptionTopicPrefix { get; }
        public string SubscriptionTopicPostfix { get; }
    }
}