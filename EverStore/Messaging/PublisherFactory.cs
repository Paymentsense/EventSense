using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal class PublisherFactory: IPublisherFactory
    {
        public async Task<PublisherClient> CreateAsync(TopicName topicName)
        {
            return await PublisherClient.CreateAsync(topicName);
        }
    }
}