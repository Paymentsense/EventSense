using System;
using System.Collections.Generic;
using System.Linq;
using EverStore.Domain;
using EverStore.Messaging;
using EverStore.Storage;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace EverStore.Tests
{
    public class EventContextTests_ReadStreamEventsForwardAsync
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async void ReadStreamEventsForwardAsync_EmptyStream_Throws(string stream)
        {
            var context = new EventContext(null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentException>(() => context.ReadStreamEventsForwardAsync(stream, 1, 1));
        }
        
        [Fact]
        public async void ReadStreamEventsForwardAsync_NegativeStart_Throws()
        {
            var context = new EventContext(null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentException>(() => context.ReadStreamEventsForwardAsync("contact_1234", -1, 1));
        }
        
        [Fact]
        public async void ReadStreamEventsForwardAsync_ZeroBatchSize_Throws()
        {
            var context = new EventContext(null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentException>(() => context.ReadStreamEventsForwardAsync("contact_1234", 1, 0));
        }
        
        [Fact]
        public async void ReadStreamEventsForwardAsync_ReturnsEvents()
        {
            var eventRepository = Substitute.For<IEventRepository>();
            var asyncCursor = Substitute.For<IAsyncCursor<PersistedEvent>>();
            asyncCursor.Current.Returns(new List<PersistedEvent> {new PersistedEvent{GlobalVersion = 10}});
            eventRepository.ReadEventsForwards(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>()).Returns(asyncCursor);
            var versionRepository = Substitute.For<IVersionRepository>();
            var eventStreamPublisher = Substitute.For<IEventStreamPublisher>();

            var context = new EventContext(null, eventStreamPublisher, versionRepository, eventRepository, null);

            var readEvents = await context.ReadStreamEventsForwardAsync("contact_1234", 1, 1);

            var @event = readEvents.Events.SingleOrDefault(e => e.GlobalVersion == 10);

            Assert.NotNull(@event);
        }
    }
}