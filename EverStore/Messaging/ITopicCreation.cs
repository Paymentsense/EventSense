using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal interface ITopicCreation
    {
        Task<TopicName> CreateAsync(string topic);
    }
}