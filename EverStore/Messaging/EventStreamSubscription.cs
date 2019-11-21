using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal class EventStreamSubscription: IEventStreamSubscription
    {
        private readonly ITopicCreation _topicCreation;
        private readonly ISubscriptionCreation _subscriptionCreation;
        private readonly IConventionIdFactory _conventionIdFactory;

        public EventStreamSubscription(ITopicCreation topicCreation, ISubscriptionCreation subscriptionCreation, IConventionIdFactory conventionIdFactory)
        {
            _topicCreation = topicCreation;
            _subscriptionCreation = subscriptionCreation;
            _conventionIdFactory = conventionIdFactory;
        }

        public async Task<SubscriptionName> CreateSubscriptionAsync(string streamAggregate)
        {
            var topicId = _conventionIdFactory.GetTopicId(streamAggregate);
            await _topicCreation.CreateAsync(topicId);

            var subscriptionId = _conventionIdFactory.GetSubscriptionId(streamAggregate);
            return await _subscriptionCreation.CreateAsync(subscriptionId, topicId);
        }
    }
}
