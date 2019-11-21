using System;
using System.Collections.Generic;
using EverStore.Contract;
using EverStore.Domain;
using EverStore.Messaging;
using EverStore.Storage;
using Google.Cloud.PubSub.V1;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace EverStore.Tests
{
    public class EventContextTests_SubscribeToStreamAsync
    {
        [Fact]
        public void SubscribeToStreamAsync_HasInvalidLastCheckpoint_Throws()
        {
            var eventContext = new EventContext(null, null, null, null, null);
            Assert.ThrowsAsync<ArgumentException>( () => eventContext.SubscribeToStreamAsync("contact_1234", -1, null, null, null));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void SubscribeToStreamAsync_HasEmptyStream_Throws(string stream)
        {
            var eventContext = new EventContext(null, null, null, null, null);
            Assert.ThrowsAsync<ArgumentException>( () => eventContext.SubscribeToStreamAsync(stream, 1, null, null, null));
        }

        [Fact]
        public void SubscribeToStreamAsync_HasInvalidStream_Throws()
        {
            var eventContext = new EventContext(null, null, null, null, null);
            Assert.ThrowsAsync<ArgumentException>(() => eventContext.SubscribeToStreamAsync("invalid stream", 1, null, null, null));
        }
        
        [Fact]
        public void SubscribeToStreamAsync_EmptyEventAppeared_Throws()
        {
            var eventContext = new EventContext(null, null, null, null, null);
            Assert.ThrowsAsync<ArgumentException>(() => eventContext.SubscribeToStreamAsync("contact_1234", 1, null, null, null));
        }

        [Fact]
        public async void SubscribeToStreamAsync_SubscribesToTheAllStream()
        {
            //Arrange
            var streamSubscriptionFactory = Substitute.For<IEventStreamSubscription>();
            streamSubscriptionFactory.CreateSubscriptionAsync(Stream.All).Returns(new SubscriptionName("projectId", "subscriptionId"));
            
            var eventRepository = Substitute.For<IEventRepository>();
            var readEvents = Substitute.For<IAsyncCursor<PersistedEvent>>();
            readEvents.MoveNext().Returns(true, true, false);
            readEvents.Current.Returns(new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 1}},
                                       new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 2}});

            eventRepository.ReadAllEventsForwards(0, Arg.Any<int>()).Returns(readEvents);

            var eventStreamSubscriber = Substitute.For<IEventStreamSubscriber>();

            var eventContext = new EventContext(eventStreamSubscriber, null, null, eventRepository, streamSubscriptionFactory);
            
            var actualEvents = new List<Tuple<CatchUpSubscription, ResolvedEvent>>();

            //Act
            await eventContext.SubscribeToStreamAsync(Stream.All, null, (c,e) => actualEvents.Add(Tuple.Create(c, e)), null, null);

            //Assert
            await eventStreamSubscriber.Received().SubscribeAsync(Stream.All,
                Arg.Any<CatchUpSubscription>(),
                3,
                Arg.Any<SubscriptionName>(),
                Arg.Any<Action<CatchUpSubscription, ResolvedEvent>>(),
                Arg.Any<Action<CatchUpSubscription>>(),
                Arg.Any<Action<CatchUpSubscription, Exception>>());

            Assert.Equal(Stream.All, actualEvents[0].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[0].Item1.SubscriptionId);
            Assert.Equal(1, actualEvents[0].Item2.GlobalVersion);

            Assert.Equal(Stream.All, actualEvents[1].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[1].Item1.SubscriptionId);
            Assert.Equal(2, actualEvents[1].Item2.GlobalVersion);
        }
        
        [Fact]
        public async void SubscribeToStreamAsync_SubscribesToTheAllStream_WithLastCheckpoint()
        {
            //Arrange
            var streamSubscriptionFactory = Substitute.For<IEventStreamSubscription>();
            streamSubscriptionFactory.CreateSubscriptionAsync(Stream.All).Returns(new SubscriptionName("projectId", "subscriptionId"));
            
            var eventRepository = Substitute.For<IEventRepository>();
            var readEvents = Substitute.For<IAsyncCursor<PersistedEvent>>();
            readEvents.MoveNext().Returns(true, true, false);
            readEvents.Current.Returns(new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 5}},
                                       new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 6}});

            eventRepository.ReadAllEventsForwards(5, Arg.Any<int>()).Returns(readEvents);

            var eventStreamSubscriber = Substitute.For<IEventStreamSubscriber>();

            var eventContext = new EventContext(eventStreamSubscriber, null, null, eventRepository, streamSubscriptionFactory);
            
            var actualEvents = new List<Tuple<CatchUpSubscription, ResolvedEvent>>();

            //Act
            await eventContext.SubscribeToStreamAsync(Stream.All, 5, (c,e) => actualEvents.Add(Tuple.Create(c, e)), null, null);

            //Assert
            await eventStreamSubscriber.Received().SubscribeAsync(Stream.All,
                Arg.Any<CatchUpSubscription>(),
                7,
                Arg.Any<SubscriptionName>(),
                Arg.Any<Action<CatchUpSubscription, ResolvedEvent>>(),
                Arg.Any<Action<CatchUpSubscription>>(),
                Arg.Any<Action<CatchUpSubscription, Exception>>());

            Assert.Equal(Stream.All, actualEvents[0].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[0].Item1.SubscriptionId);
            Assert.Equal(5, actualEvents[0].Item2.GlobalVersion);

            Assert.Equal(Stream.All, actualEvents[1].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[1].Item1.SubscriptionId);
            Assert.Equal(6, actualEvents[1].Item2.GlobalVersion);
        }

        [Fact]
        public async void SubscribeToStreamAsync_SubscribesToAStream()
        {
            //Arrange
            var streamSubscriptionFactory = Substitute.For<IEventStreamSubscription>();
            streamSubscriptionFactory.CreateSubscriptionAsync("contact").Returns(new SubscriptionName("projectId", "subscriptionId"));
            
            var eventRepository = Substitute.For<IEventRepository>();
            var readEvents = Substitute.For<IAsyncCursor<PersistedEvent>>();
            readEvents.MoveNext().Returns(true, true, false);
            readEvents.Current.Returns(new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 10, StreamVersion = 1}},
                                       new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 12, StreamVersion = 2}});

            eventRepository.ReadEventsForwards("contact_1234", 0, Arg.Any<int>()).Returns(readEvents);

            var eventStreamSubscriber = Substitute.For<IEventStreamSubscriber>();

            var eventContext = new EventContext(eventStreamSubscriber, null, null, eventRepository, streamSubscriptionFactory);
            
            var actualEvents = new List<Tuple<CatchUpSubscription, ResolvedEvent>>();

            //Act
            await eventContext.SubscribeToStreamAsync("contact_1234", null, (c,e) => actualEvents.Add(Tuple.Create(c, e)), null, null);

            //Assert
            await eventStreamSubscriber.Received().SubscribeAsync("contact_1234",
                Arg.Any<CatchUpSubscription>(),
                3,
                Arg.Any<SubscriptionName>(),
                Arg.Any<Action<CatchUpSubscription, ResolvedEvent>>(),
                Arg.Any<Action<CatchUpSubscription>>(),
                Arg.Any<Action<CatchUpSubscription, Exception>>());

            Assert.Equal("contact_1234", actualEvents[0].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[0].Item1.SubscriptionId);
            Assert.Equal(10, actualEvents[0].Item2.GlobalVersion);

            Assert.Equal("contact_1234", actualEvents[1].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[1].Item1.SubscriptionId);
            Assert.Equal(12, actualEvents[1].Item2.GlobalVersion);
        }
        
        [Fact]
        public async void SubscribeToStreamAsync_SubscribesToAStreamWithLastCheckpoint()
        {
            //Arrange
            var streamSubscriptionFactory = Substitute.For<IEventStreamSubscription>();
            streamSubscriptionFactory.CreateSubscriptionAsync("contact").Returns(new SubscriptionName("projectId", "subscriptionId"));
            
            var eventRepository = Substitute.For<IEventRepository>();
            var readEvents = Substitute.For<IAsyncCursor<PersistedEvent>>();
            readEvents.MoveNext().Returns(true, true, false);
            readEvents.Current.Returns(new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 10, StreamVersion = 5}},
                                       new List<PersistedEvent>() {new PersistedEvent {GlobalVersion = 12, StreamVersion = 6}});

            eventRepository.ReadEventsForwards("contact_1234", 5, Arg.Any<int>()).Returns(readEvents);

            var eventStreamSubscriber = Substitute.For<IEventStreamSubscriber>();

            var eventContext = new EventContext(eventStreamSubscriber, null, null, eventRepository, streamSubscriptionFactory);
            
            var actualEvents = new List<Tuple<CatchUpSubscription, ResolvedEvent>>();

            //Act
            await eventContext.SubscribeToStreamAsync("contact_1234", 5, (c,e) => actualEvents.Add(Tuple.Create(c, e)), null, null);

            //Assert
            await eventStreamSubscriber.Received().SubscribeAsync("contact_1234",
                Arg.Any<CatchUpSubscription>(),
                7,
                Arg.Any<SubscriptionName>(),
                Arg.Any<Action<CatchUpSubscription, ResolvedEvent>>(),
                Arg.Any<Action<CatchUpSubscription>>(),
                Arg.Any<Action<CatchUpSubscription, Exception>>());

            Assert.Equal("contact_1234", actualEvents[0].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[0].Item1.SubscriptionId);
            Assert.Equal(10, actualEvents[0].Item2.GlobalVersion);

            Assert.Equal("contact_1234", actualEvents[1].Item1.SubscribedStream);
            Assert.Equal("subscriptionId", actualEvents[1].Item1.SubscriptionId);
            Assert.Equal(12, actualEvents[1].Item2.GlobalVersion);
        }

        [Fact]
        public async void SubscribeToStreamAsync_SubscribesToAStream_NewStream()
        {
            //Arrange
            var streamSubscriptionFactory = Substitute.For<IEventStreamSubscription>();
            streamSubscriptionFactory.CreateSubscriptionAsync("contact").Returns(new SubscriptionName("projectId", "subscriptionId"));

            var eventRepository = Substitute.For<IEventRepository>();
            var readEvents = Substitute.For<IAsyncCursor<PersistedEvent>>();
            readEvents.MoveNext().Returns(false);

            eventRepository.ReadEventsForwards("contact_1234", 0, Arg.Any<int>()).Returns(readEvents);

            var eventStreamSubscriber = Substitute.For<IEventStreamSubscriber>();

            var eventContext = new EventContext(eventStreamSubscriber, null, null, eventRepository, streamSubscriptionFactory);

            //Act
            await eventContext.SubscribeToStreamAsync("contact_1234", null, (c,e) => { }, null, null);

            //Assert
            await eventStreamSubscriber.Received().SubscribeAsync("contact_1234",
                Arg.Any<CatchUpSubscription>(),
                1,
                Arg.Any<SubscriptionName>(),
                Arg.Any<Action<CatchUpSubscription, ResolvedEvent>>(),
                Arg.Any<Action<CatchUpSubscription>>(),
                Arg.Any<Action<CatchUpSubscription, Exception>>());

        }

    }
}
