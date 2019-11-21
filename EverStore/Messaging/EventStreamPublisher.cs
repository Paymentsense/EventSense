﻿using System.Text;
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
        private readonly ITopicCreation _topicCreation;
        private readonly ITracer _tracer;
        private readonly IPublisherFactory _publisherFactory;
        private readonly IConventionIdFactory _conventionIdFactory;

        public EventStreamPublisher(ITopicCreation topicCreation, ITracer tracer, IPublisherFactory publisherFactory, IConventionIdFactory conventionIdFactory)
        {
            _topicCreation = topicCreation;
            _tracer = tracer;
            _publisherFactory = publisherFactory;
            _conventionIdFactory = conventionIdFactory;
        }

        public async Task<string> Publish(PersistedEvent @event, string stream, string streamAggregate, string streamId)
        {
            var topicId = _conventionIdFactory.GetTopicId(streamAggregate);
            var topicName = await _topicCreation.CreateAsync(topicId);
            var publisher = await _publisherFactory.CreateAsync(topicName);

            var publisherSpan = _tracer?.CreateSpan(Encoding.UTF8.GetString(@event.Data), topicName.TopicId);
            try
            {
                var pubsubMessage = new PubsubMessage
                {
                    Data = ByteString.CopyFrom(@event.Data),
                    Attributes =
                    {
                        {EventStreamAttributes.GlobalVersion, @event.GlobalVersion.ToString()},
                        {EventStreamAttributes.Stream, stream},
                        {EventStreamAttributes.StreamId, streamId},
                        {EventStreamAttributes.StreamAggregate, streamAggregate},
                        {EventStreamAttributes.StreamVersion, @event.StreamVersion.ToString()},
                        {EventStreamAttributes.CreatedAt, @event.CreatedAt.ToString()}
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