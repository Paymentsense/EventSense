using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Grpc.Core;

namespace EverStore.Messaging
{
    internal class SubscriptionCreation: ISubscriptionCreation
    {
        private readonly string _projectId;

        public SubscriptionCreation(string projectId)
        {
            _projectId = projectId;
        }

        public async Task<SubscriptionName> CreateAsync(string subscriptionId, string topicId)
        {
            var topicName = new TopicName(_projectId, topicId);
            var subscriptionName = new SubscriptionName(_projectId, subscriptionId);

            var subscriberClient = SubscriberServiceApiClient.Create();
            try
            {
                await subscriberClient.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: null);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
            }

            return subscriptionName;
        }
    }
}