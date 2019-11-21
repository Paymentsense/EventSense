using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface ISubscriptionCreation
    {
        Task<SubscriptionName> CreateAsync(string subscriptionId, string topicId);
    }
}