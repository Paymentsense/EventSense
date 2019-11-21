using EverStore.Messaging;
using Google.Cloud.PubSub.V1;
using NSubstitute;
using Xunit;

namespace EverStore.Tests.Messaging
{
    public class EventStreamSubscriptionTests
    {
        [Fact]
        public async void CreatesTopicsAndSubscription()
        {
            //Arrange
            var topicCreation = Substitute.For<ITopicCreation>();
            var subscriptionCreation = Substitute.For<ISubscriptionCreation>();
            var subscriptionName = new SubscriptionName("project", "subscriptionId");
            subscriptionCreation.CreateAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(subscriptionName);
            var conventionIdFactory = Substitute.For<IConventionIdFactory>();
            conventionIdFactory.GetSubscriptionId(Arg.Any<string>()).Returns("subscriptionId");
            conventionIdFactory.GetTopicId(Arg.Any<string>()).Returns("topicId");

            var eventStreamSubscription = new EventStreamSubscription(topicCreation, subscriptionCreation, conventionIdFactory);

            //Act
            var subscription = await eventStreamSubscription.CreateSubscriptionAsync("stream");

            //Assert
            Assert.Equal("subscriptionId", subscription.SubscriptionId);

            await topicCreation.Received().CreateAsync(Arg.Any<string>());
        }
    }
}
