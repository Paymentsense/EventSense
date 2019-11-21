using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface IEventStreamSubscriptionCreation
    {
        Task<SubscriptionName> CreateSubscriptionAsync(string streamAggregate);
    }
}