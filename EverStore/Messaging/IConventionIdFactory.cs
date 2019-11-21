namespace EverStore.Messaging
{
    public interface IConventionIdFactory
    {
        string GetTopicId(string streamAggregate);
        string GetSubscriptionId(string streamAggregate);
    }
}