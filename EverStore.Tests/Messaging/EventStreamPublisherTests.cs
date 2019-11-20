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
            var pubSubPublisherFactory = Substitute.For<IPubSubPublisherFactory>();
            var publisherClient = Substitute.For<PublisherClient>();
            var topicFactory = Substitute.For<ITopicFactory>();
            topicFactory.CreateAsync(Arg.Any<string>()).Returns(new TopicName("project", "topic"));
            PubsubMessage message = null;
            await publisherClient.PublishAsync(Arg.Do<PubsubMessage>(m => message = m));

            pubSubPublisherFactory.CreateAsync(default).ReturnsForAnyArgs(Task.FromResult(publisherClient));
            
            var publisher = new EventStreamPublisher(topicFactory, null, pubSubPublisherFactory);

            var persistedEvent = new PersistedEvent {Data = Encoding.UTF8.GetBytes("event")};

            await publisher.Publish(persistedEvent, "stream_1", "stream", "1");

            Assert.NotNull(message);
            Assert.Equal("event", message.Data?.ToStringUtf8());

            Assert.Equal("stream_1", message.Attributes[EventStreamAttributes.Stream]);
            Assert.Equal("stream", message.Attributes[EventStreamAttributes.StreamAggregate]);
            Assert.Equal("1", message.Attributes[EventStreamAttributes.StreamId]);
        }
        
        [Fact]
        public async void PublishesEventWithTracingAttributes()
        {
            var pubSubPublisherFactory = Substitute.For<IPubSubPublisherFactory>();
            var publisherClient = Substitute.For<PublisherClient>();
            var topicFactory = Substitute.For<ITopicFactory>();
            topicFactory.CreateAsync(Arg.Any<string>()).Returns(new TopicName("project", "topic"));
            PubsubMessage message = null;

            await publisherClient.PublishAsync(Arg.Do<PubsubMessage>(m => message = m));

            pubSubPublisherFactory.CreateAsync(default).ReturnsForAnyArgs(Task.FromResult(publisherClient));

            var tracer = Substitute.For<ITracer>();
            tracer.Inject(Arg.Any<ISpanContext>(), Arg.Any<IFormat<ITextMap>>(), Arg.Do<TextMapInjectAdapter>(a => a.Set("tracer", "trace this")));
            var publisher = new EventStreamPublisher(topicFactory, tracer, pubSubPublisherFactory);

            var persistedEvent = new PersistedEvent {Data = Encoding.UTF8.GetBytes("event")};

            await publisher.Publish(persistedEvent, "stream_1", "stream", "1");

            Assert.NotNull(message);
            Assert.Equal("trace this", message.Attributes["tracer"]);
        }
    }
}
