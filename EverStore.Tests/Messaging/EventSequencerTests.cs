using EverStore.Domain;
using EverStore.Messaging;
using Xunit;

namespace EverStore.Tests.Messaging
{
    public class EventSequencerTests
    {
        [Fact]
        public void GetEventSequence_EventForStreamIsInSequence()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = 1,
                GlobalVersion = -1
            };

            //Act
            sequencer.Initialise(1);
            var sequence = sequencer.GetEventSequence(persistedEvent, false);

            //Assert
            Assert.False(sequence.IsInPast);
        }
        
        [Fact]
        public void GetEventSequence_EventForStreamIsInThePast()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = 1,
                GlobalVersion = -1
            };

            //Act
            sequencer.Initialise(2);
            var sequence = sequencer.GetEventSequence(persistedEvent, false);

            //Assert
            Assert.True(sequence.IsInPast);
        }
        
        [Fact]
        public void GetEventSequence_EventForAllStreamIsInSequence()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = -1,
                GlobalVersion = 1
            };

            //Act
            sequencer.Initialise(1);
            var sequence = sequencer.GetEventSequence(persistedEvent,  true);

            //Assert
            Assert.False(sequence.IsInPast);
        }
     
        [Fact]
        public void GetEventSequence_EventForAllStreamIsInThePast()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = -1,
                GlobalVersion = 1
            };

            //Act
            sequencer.Initialise(2);
            var sequence = sequencer.GetEventSequence(persistedEvent, true);

            //Assert
            Assert.True(sequence.IsInPast);
        }
        
        [Fact]
        public void GetEventSequence_EventForAllStreamIsFirst()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = -1,
                GlobalVersion = 1
            };

            //Act
            sequencer.Initialise(1);
            var sequence = sequencer.GetEventSequence(persistedEvent, true);

            //Assert
            Assert.True(sequence.IsFirstEvent);
        }

        [Fact]
        public void GetEventSequence_EventForStreamIsFirst()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = 1,
                GlobalVersion = -1
            };

            //Act
            sequencer.Initialise(1);
            var sequence = sequencer.GetEventSequence(persistedEvent, false);

            //Assert
            Assert.True(sequence.IsFirstEvent);
        }
    }
}
