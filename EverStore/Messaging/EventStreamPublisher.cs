using System.Text;
using System.Threading.Tasks;
using EverStore.Domain;
using EverStore.Tracing;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using OpenTracing;

namespace EverStore.Messaging
{

    internal class EventStreamPublisher : IEventStreamPublisher
    {
        private readonly ITopicFactory _topicFactory;
        private readonly ITracer _tracer;
        private readonly IPubSubPublisherFactory _pubSubPublisherFactory;

        public EventStreamPublisher(ITopicFactory topicFactory, ITracer tracer, IPubSubPublisherFactory pubSubPublisherFactory )
        {
            _topicFactory = topicFactory;
            _tracer = tracer;
            _pubSubPublisherFactory = pubSubPublisherFactory;
        }

        public async Task<string> Publish(PersistedEvent @event, string stream, string streamAggregate, string streamId)
        {
            var topicName = await _topicFactory.CreateAsync(streamAggregate);
            var publisher = await _pubSubPublisherFactory.CreateAsync(topicName);

            var publisherSpan = _tracer?.CreateSpan(Encoding.UTF8.GetString(@event.Data), topicName.TopicId);
            try
            {
                var pubsubMessage = new PubsubMessage
                {
                    Data = ByteString.CopyFrom(@event.Data),
                    Attributes =
                    {
                        {EventStreamAttributes.Stream, stream},
                        {EventStreamAttributes.StreamId, streamId},
                        {EventStreamAttributes.StreamAggregate, streamAggregate}
                    }
                };

                if (_tracer != null)
                {
                    pubsubMessage.Attributes.Add(_tracer.CreateSpanInformationCarrier());
                }

                return await publisher.PublishAsync(pubsubMessage);
            }
            finally
            {
                publisherSpan?.Finish();
            }
        }
    }
}