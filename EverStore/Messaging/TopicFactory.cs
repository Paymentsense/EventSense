using Google.Cloud.PubSub.V1;

namespace EverStore.Messaging
{
    internal class TopicFactory
    {
        private readonly string _projectId;

        public TopicFactory(string projectId)
        {
            _projectId = projectId;
        }

        public TopicName Create(string topic)
        {
            return new TopicName(_projectId, topic);
        }
    }
}
