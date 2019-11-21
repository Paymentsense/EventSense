using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Grpc.Core;

namespace EverStore.Messaging
{
    internal class TopicCreation : ITopicCreation
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<string, byte> _createdTopics = new ConcurrentDictionary<string, byte>();
        private readonly string _projectId;

        public TopicCreation(string projectId)
        {
            _projectId = projectId;
        }

        public async Task<TopicName> CreateAsync(string topic)
        {
            var topicName = new TopicName(_projectId, topic);

            if (!_createdTopics.ContainsKey(topic))
            {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    if (!_createdTopics.ContainsKey(topic))
                    {
                        var publisherClient = PublisherServiceApiClient.Create();
                        try
                        {
                            await publisherClient.CreateTopicAsync(topicName);
                        }
                        catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
                        {
                        }
                    }
                    _createdTopics.TryAdd(topic, 0);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }

            return topicName;
        }
    }
}
