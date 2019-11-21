using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface IPublisherFactory
    {
        Task<PublisherClient> CreateAsync(TopicName topicName);
    }
}