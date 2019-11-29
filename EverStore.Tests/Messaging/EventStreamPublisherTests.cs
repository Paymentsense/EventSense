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
            //Arrange
            var pubSubPublisherFactory = Substitute.For<IPublisherFactory>();
            var conventionIdFactory = Substitute.For<IConventionIdFactory>();
            var topicCreation = Substitute.For<ITopicCreation>();

            conventionIdFactory.GetTopicId("stream").Returns("stream");
            var aggregateTopic = new TopicName("project", "aggregateTopic");
            topicCreation.CreateAsync("stream").Returns(aggregateTopic);
            PubsubMessage aggregateMessage = null;
            var aggregatePublisherClient = Substitute.For<PublisherClient>();
            await aggregatePublisherClient.PublishAsync(Arg.Do<PubsubMessage>(m => aggregateMessage = m));
            pubSubPublisherFactory.CreateAsync(aggregateTopic).Returns(Task.FromResult(aggregatePublisherClient));

            conventionIdFactory.GetTopicId(Stream.All).Returns(Stream.All);
            var allTopic = new TopicName("project", "allTopic");
            topicCreation.CreateAsync(Stream.All).Returns(allTopic);
            PubsubMessage allMessage = null;
            var allPublisherClient = Substitute.For<PublisherClient>();
            await allPublisherClient.PublishAsync(Arg.Do<PubsubMessage>(m => allMessage = m));
            pubSubPublisherFactory.CreateAsync(allTopic).Returns(Task.FromResult(allPublisherClient));

            var publisher = new EventStreamPublisher(topicCreation, null, pubSubPublisherFactory, conventionIdFactory);

            var createdAt = new DateTimeOffset(2019, 11, 21, 9, 20, 0, TimeSpan.FromHours(1));
            var persistedEvent = new PersistedEvent
            {
                GlobalVersion = 12,
                StreamVersion = 8,
                Data = Encoding.UTF8.GetBytes("event"), 
                CreatedAt = createdAt
            };

            //Act
            await publisher.Publish(persistedEvent, "stream_1", "stream", "1");

            //Assert
            Assert.NotNull(allMessage);
            Assert.Equal("event", allMessage.Data?.ToStringUtf8());

            Assert.Equal("stream_1", allMessage.Attributes[EventStreamAttributes.Stream]);
            Assert.Equal("stream", allMessage.Attributes[EventStreamAttributes.StreamAggregate]);
            Assert.Equal("1", allMessage.Attributes[EventStreamAttributes.StreamId]);
            Assert.Equal("8", allMessage.Attributes[EventStreamAttributes.StreamVersion]);
            Assert.Equal("12", allMessage.Attributes[EventStreamAttributes.GlobalVersion]);
            Assert.Equal(createdAt, DateTimeOffset.Parse(allMessage.Attributes[EventStreamAttributes.CreatedAt]));
            
            Assert.NotNull(aggregateMessage);
            Assert.Equal("event", aggregateMessage.Data?.ToStringUtf8());

            Assert.Equal("stream_1", aggregateMessage.Attributes[EventStreamAttributes.Stream]);
            Assert.Equal("stream", aggregateMessage.Attributes[EventStreamAttributes.StreamAggregate]);
            Assert.Equal("1", aggregateMessage.Attributes[EventStreamAttributes.StreamId]);
            Assert.Equal("8", aggregateMessage.Attributes[EventStreamAttributes.StreamVersion]);
            Assert.Equal("12", aggregateMessage.Attributes[EventStreamAttributes.GlobalVersion]);
            Assert.Equal(createdAt, DateTimeOffset.Parse(aggregateMessage.Attributes[EventStreamAttributes.CreatedAt]));
        }
        
        [Fact]
        public async void PublishesEventWithTracingAttributes()
        {
            //Arrange
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

            //Act
            await publisher.Publish(persistedEvent, "stream_1", "stream", "1");

            //Assert
            Assert.NotNull(message);
            Assert.Equal("trace this", message.Attributes["tracer"]);
        }
    }
}
