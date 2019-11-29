using System;

namespace EverStore.Contract
{
    public class EventContextSettings
    {
        public EventContextSettings(string gcpProjectId, string eventStorageName, string subscriptionTopicPrefix, string subscriptionTopicPostfix, string subscriptionIdentifier = null)
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

        public void CheckSettingsAreValid()
        {
            if (string.IsNullOrEmpty(GcpProjectId))
            {
                throw new ArgumentException($"{nameof(GcpProjectId)} is empty", nameof(GcpProjectId));
            }
            if (string.IsNullOrEmpty(EventStorageName))
            {
                throw new ArgumentException($"{nameof(EventStorageName)} is empty", nameof(EventStorageName));
            }
            if (string.IsNullOrEmpty(SubscriptionIdentifier))
            {
                throw new ArgumentException($"{nameof(SubscriptionIdentifier)} is empty", nameof(SubscriptionIdentifier));
            }
            if (string.IsNullOrEmpty(SubscriptionTopicPrefix))
            {
                throw new ArgumentException($"{nameof(SubscriptionTopicPrefix)} is empty", nameof(SubscriptionTopicPrefix));
            }
            if (string.IsNullOrEmpty(SubscriptionTopicPostfix))
            {
                throw new ArgumentException($"{nameof(SubscriptionTopicPostfix)} is empty", nameof(SubscriptionTopicPostfix));
            }
        }
    }
}