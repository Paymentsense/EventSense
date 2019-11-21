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
            Assert.True(sequence.IsInSequence);
            Assert.False(sequence.IsInPast);
        }
        
        [Fact]
        public void GetEventSequence_EventForStreamIsNotInSequence()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = 2,
                GlobalVersion = -1
            };

            //Act
            sequencer.Initialise(1);
            var sequence = sequencer.GetEventSequence(persistedEvent, false);

            //Assert
            Assert.False(sequence.IsInSequence);
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
            Assert.False(sequence.IsInSequence);
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
            Assert.True(sequence.IsInSequence);
            Assert.False(sequence.IsInPast);
        }
        
        [Fact]
        public void GetEventSequence_EventForAllStreamIsNotInSequence()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var persistedEvent = new PersistedEvent
            {
                StreamVersion = -1,
                GlobalVersion = 2
            };

            //Act
            sequencer.Initialise(1);
            var sequence = sequencer.GetEventSequence(persistedEvent, true);

            //Assert
            Assert.False(sequence.IsInSequence);
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
            Assert.False(sequence.IsInSequence);
            Assert.True(sequence.IsInPast);
        }

        [Fact]
        public void IncrementEventSequence_EventForStreamIsInSequence()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var firstEvent = new PersistedEvent
            {
                StreamVersion = 1,
                GlobalVersion = -1
            };

            var secondEvent = new PersistedEvent
            {
                StreamVersion = 2,
                GlobalVersion = -1
            };

            //Act
            sequencer.Initialise(1);
            var firstSequence = sequencer.GetEventSequence(firstEvent, false);
            sequencer.IncrementEventSequence();
            var secondSequence = sequencer.GetEventSequence(secondEvent,  false);

            //Arrange
            Assert.True(firstSequence.IsInSequence);
            Assert.False(firstSequence.IsInPast);
            
            Assert.True(secondSequence.IsInSequence);
            Assert.False(secondSequence.IsInPast);
        }

        [Fact]
        public void IncrementEventSequence_EventForStreamIsInSequence_LaterCheckpoint()
        {
            //Arrange
            var sequencer = new EventSequencer();
            var firstEvent = new PersistedEvent
            {
                StreamVersion = 5,
                GlobalVersion = -1
            };

            var secondEvent = new PersistedEvent
            {
                StreamVersion = 6,
                GlobalVersion = -1
            };

            //Act
            sequencer.Initialise(5);
            var firstSequence = sequencer.GetEventSequence(firstEvent,  false);
            sequencer.IncrementEventSequence();
            var secondSequence = sequencer.GetEventSequence(secondEvent,  false);

            //Arrange
            Assert.True(firstSequence.IsInSequence);
            Assert.False(firstSequence.IsInPast);
            
            Assert.True(secondSequence.IsInSequence);
            Assert.False(secondSequence.IsInPast);
        }
    }
}
