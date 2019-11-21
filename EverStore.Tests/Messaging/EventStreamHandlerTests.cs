using System;
using EverStore.Contract;
using EverStore.Domain;
using EverStore.Messaging;
using Google.Cloud.PubSub.V1;
using NSubstitute;
using Xunit;

namespace EverStore.Tests.Messaging
{
    public class EventStreamHandlerTests
    {
        [Fact]
        public void HandleEvents_InSequence_Success()
        {
            //Arrange
            var @event = new PersistedEvent{GlobalVersion = 1};
            var catchUpSubscription = new CatchUpSubscription("stream", "subId");
            var eventStreamSubscription = new EventStreamSubscription(null, catchUpSubscription, 1, null,false);

            var eventSequencer = Substitute.For<IEventSequencer>();
            var eventSequence = new EventSequence(isInSequence:true, isInPast: false);
            eventSequencer.GetEventSequence(Arg.Any<PersistedEvent>(), 1, Arg.Any<bool>()).Returns(eventSequence);
            var handler = new EventStreamHandler(eventSequencer);

            Tuple<CatchUpSubscription, ResolvedEvent> eventAppeared = null;

            //Act
            var response = handler.Handle(@event, eventStreamSubscription, (c, e) => eventAppeared = new Tuple<CatchUpSubscription, ResolvedEvent>(c, e), null);

            //Arrange
            Assert.Equal(SubscriberClient.Reply.Ack, response);
            Assert.NotNull(eventAppeared);
            Assert.Equal(catchUpSubscription, eventAppeared.Item1);
            Assert.Equal(1, eventAppeared.Item2.GlobalVersion);
            eventSequencer.Received().IncrementEventSequence();
        }
        
        [Fact]
        public void HandlesEvents_FutureOutOfSequence_Fails()
        {
            //Arrange
            var @event = new PersistedEvent{GlobalVersion = 2};
            var catchUpSubscription = new CatchUpSubscription("stream", "subId");
            var eventStreamSubscription = new EventStreamSubscription(null, catchUpSubscription, 1, null,false);

            var eventSequencer = Substitute.For<IEventSequencer>();
            var eventSequence = new EventSequence(isInSequence:false, isInPast: false);
            eventSequencer.GetEventSequence(Arg.Any<PersistedEvent>(), 1, Arg.Any<bool>()).Returns(eventSequence);
            var handler = new EventStreamHandler(eventSequencer);

            Tuple<CatchUpSubscription, ResolvedEvent> eventAppeared = null;

            //Act
            var response = handler.Handle(@event, eventStreamSubscription, (c, e) => eventAppeared = new Tuple<CatchUpSubscription, ResolvedEvent>(c, e), null);

            //Arrange
            Assert.Equal(SubscriberClient.Reply.Nack, response);
            Assert.Null(eventAppeared);
            eventSequencer.DidNotReceive().IncrementEventSequence();
        }

        [Fact]
        public void HandlesEvents_InThePastOutOfSequence_Ignores()
        {
            //Arrange
            var @event = new PersistedEvent{GlobalVersion = 2};
            var catchUpSubscription = new CatchUpSubscription("stream", "subId");
            var eventStreamSubscription = new EventStreamSubscription(null, catchUpSubscription, 1, null,false);

            var eventSequencer = Substitute.For<IEventSequencer>();
            var eventSequence = new EventSequence(isInSequence:false, isInPast: true);
            eventSequencer.GetEventSequence(Arg.Any<PersistedEvent>(), 1, Arg.Any<bool>()).Returns(eventSequence);
            var handler = new EventStreamHandler(eventSequencer);

            Tuple<CatchUpSubscription, ResolvedEvent> eventAppeared = null;

            //Act
            var response = handler.Handle(@event, eventStreamSubscription, (c, e) => eventAppeared = new Tuple<CatchUpSubscription, ResolvedEvent>(c, e), null);

            //Arrange
            Assert.Equal(SubscriberClient.Reply.Ack, response);
            Assert.Null(eventAppeared);
        }

        [Fact]
        public void HandleFirstEvents_InSequence_LiveStreamingStarted()
        {
            //Arrange
            var @event = new PersistedEvent { GlobalVersion = 1 };
            var catchUpSubscription = new CatchUpSubscription("stream", "subId");
            var eventStreamSubscription = new EventStreamSubscription(null, catchUpSubscription, 1, null, false);

            var eventSequencer = Substitute.For<IEventSequencer>();
            var eventSequence = new EventSequence(isInSequence: true, isInPast: false);
            eventSequencer.GetEventSequence(Arg.Any<PersistedEvent>(), 1, Arg.Any<bool>()).Returns(eventSequence);
            eventSequencer.IsFirstEvent().Returns(true);

            var handler = new EventStreamHandler(eventSequencer);

            Tuple<CatchUpSubscription, ResolvedEvent> eventAppeared = null;
            CatchUpSubscription liveProcessingStarted = null;

            //Act
            var response = handler.Handle(@event, eventStreamSubscription, (c, e) => eventAppeared = new Tuple<CatchUpSubscription, ResolvedEvent>(c, e), c => liveProcessingStarted = c);

            //Arrange
            Assert.Equal(SubscriberClient.Reply.Ack, response);
            Assert.NotNull(eventAppeared);
            Assert.NotNull(liveProcessingStarted);
            Assert.Equal(catchUpSubscription, liveProcessingStarted);
        }
    }
}
