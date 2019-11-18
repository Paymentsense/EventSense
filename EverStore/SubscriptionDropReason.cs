namespace EverStore
{
    public enum SubscriptionDropReason
    {
        Unsubscribed = 0,
        AccessDenied = 1,
        NotFound = 2,
        PersistentSubscriptionDeleted = 3,
        SubscriberMaxCountReached = 4
    }
}