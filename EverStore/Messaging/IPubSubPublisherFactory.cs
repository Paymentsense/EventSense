using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface IPubSubPublisherFactory
    {
        Task<PublisherClient> CreateAsync(TopicName topicName);
    }
}