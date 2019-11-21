using System;
using System.Text;
using System.Threading.Tasks;
using EverStore.Domain;
using EverStore.Messaging;
using Google.Cloud.PubSub.V1;
using NSubstitute;
using OpenTracing;
using OpenTracing.Propagation;
using Xunit;

namespace EverStore.Tests.Messaging
{
    public class EventStreamPublisherTests
    {
        [Fact]
        public async void PublishesEventWithAttributes()
        {
            var pubSubPublisherFactory = Substitute.For<IPublisherFactory>();
            var publisherClient = Substitute.For<PublisherClient>();
            var conventionIdFactory = Substitute.For<IConventionIdFactory>();
            var topicCreation = Substitute.For<ITopicCreation>();
            topicCreation.CreateAsync(Arg.Any<string>()).Returns(new TopicName("project", "topic"));
            PubsubMessage message = null;
            await publisherClient.PublishAsync(Arg.Do<PubsubMessage>(m => message = m));

            pubSubPublisherFactory.CreateAsync(default).ReturnsForAnyArgs(Task.FromResult(publisherClient));
            
            var publisher = new EventStreamPublisher(topicCreation, null, pubSubPublisherFactory, conventionIdFactory);

            var createdAt = new DateTimeOffset(2019, 11, 21, 9, 20, 0, TimeSpan.FromHours(1));
            var persistedEvent = new PersistedEvent
            {
                GlobalVersion = 12,
                StreamVersion = 8,
                Data = Encoding.UTF8.GetBytes("event"), 
                CreatedAt = createdAt
            };

            await publisher.Publish(persistedEvent, "stream_1", "stream", "1");

            Assert.NotNull(message);
            Assert.Equal("event", message.Data?.ToStringUtf8());

            Assert.Equal("stream_1", message.Attributes[EventStreamAttributes.Stream]);
            Assert.Equal("stream", message.Attributes[EventStreamAttributes.StreamAggregate]);
            Assert.Equal("1", message.Attributes[EventStreamAttributes.StreamId]);
            Assert.Equal("8", message.Attributes[EventStreamAttributes.StreamVersion]);
            Assert.Equal("12", message.Attributes[EventStreamAttributes.GlobalVersion]);
            Assert.Equal(createdAt, DateTimeOffset.Parse(message.Attributes[EventStreamAttributes.CreatedAt]));
        }
        
        [Fact]
        public async void PublishesEventWithTracingAttributes()
        {
            var pubSubPublisherFactory = Substitute.For<IPublisherFactory>();
            var publisherClient = Substitute.For<PublisherClient>();
            var topicFactory = Substitute.For<ITopicCreation>();
            var conventionIdFactory = Substitute.For<IConventionIdFactory>();
            topicFactory.CreateAsync(Arg.Any<string>()).Returns(new TopicName("project", "topic"));
            PubsubMessage message = null;

            await publisherClient.PublishAsync(Arg.Do<PubsubMessage>(m => message = m));

            pubSubPublisherFactory.CreateAsync(default).ReturnsForAnyArgs(Task.FromResult(publisherClient));

            var tracer = Substitute.For<ITracer>();
            tracer.Inject(Arg.Any<ISpanContext>(), Arg.Any<IFormat<ITextMap>>(), Arg.Do<TextMapInjectAdapter>(a => a.Set("tracer", "trace this")));
            var publisher = new EventStreamPublisher(topicFactory, tracer, pubSubPublisherFactory, conventionIdFactory);

            var persistedEvent = new PersistedEvent {Data = Encoding.UTF8.GetBytes("event")};

            await publisher.Publish(persistedEvent, "stream_1", "stream", "1");

            Assert.NotNull(message);
            Assert.Equal("trace this", message.Attributes["tracer"]);
        }
    }
}
