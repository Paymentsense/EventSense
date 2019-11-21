using System;
using System.Text;
using EverStore.Contract;
using EverStore.Domain;
using EverStore.Messaging;
using EverStore.Storage;
using NSubstitute;
using Xunit;

namespace EverStore.Tests
{
    public class EventContextTests_AppendToStreamAsync
    {
        private const string ValidEventData = "Valid Event";
        private readonly Event _validEvent = new Event {Data = Encoding.UTF8.GetBytes(ValidEventData)};

        [Fact]
        public async void AppendToStreamAsync_StreamIsValid()
        {
            var context = new EventContext(null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentException>(() => context.AppendToStreamAsync("", 0, _validEvent));
        }
        
        [Fact]
        public async void AppendToStreamAsync_StreamVersionIsNegative()
        {
            var context = new EventContext(null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentException>(() => context.AppendToStreamAsync("contact_123", -1, _validEvent));
        }

        [Fact]
        public async void AppendToStreamAsync_NoEvent()
        {
            var context = new EventContext(null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentException>(() => context.AppendToStreamAsync("contact_123", 0, null));
        }

        [Fact]
        public async void AppendToStreamAsync_NoEventData()
        {
            var @event = new Event{ Data = null };

            var context = new EventContext(null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentException>(() => context.AppendToStreamAsync("contact_123", 0, @event));
        }
        
        [Fact]
        public async void AppendToStreamAsync_EmptyEventData()
        {
            var @event = new Event { Data = new byte[0] };

            var context = new EventContext(null, null, null, null, null);
            await Assert.ThrowsAsync<ArgumentException>(() => context.AppendToStreamAsync("contact_123", 0, @event));
        }

        [Fact]
        public async void AppendToStreamAsync_EventIsAppended()
        {
            var eventRepository = Substitute.For<IEventRepository>();
            PersistedEvent persistedEvent = null;
            eventRepository.AppendEvent(Arg.Do<PersistedEvent>(e => persistedEvent = e));

            var versionRepository = Substitute.For<IVersionRepository>();
            versionRepository.GetNextGlobalVersion().Returns(2);

            var eventStreamPublisher = Substitute.For<IEventStreamPublisher>();
            var context = new EventContext(null, eventStreamPublisher, versionRepository, eventRepository , null);

            var @event = await context.AppendToStreamAsync("contact_1234", 1, _validEvent);

            Assert.NotNull(persistedEvent);

            await eventStreamPublisher.Received().Publish(Arg.Is<PersistedEvent>(persistedEvent), Arg.Is("contact_1234"), Arg.Is("contact"), Arg.Is("1234"));

            Assert.NotNull(@event);
            Assert.Equal(2, @event.GlobalVersion);
            Assert.Equal(1, @event.StreamVersion);
            Assert.Equal("contact_1234", @event.Stream);
            Assert.Equal(ValidEventData, Encoding.UTF8.GetString(@event.Data));
        }
    }
}
