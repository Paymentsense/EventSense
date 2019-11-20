using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface ITopicFactory
    {
        Task<TopicName> CreateAsync(string topic);
    }
}