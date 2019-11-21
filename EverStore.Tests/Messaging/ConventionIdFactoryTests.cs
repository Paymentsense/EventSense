using EverStore.Messaging;
using Xunit;

namespace EverStore.Tests.Messaging
{
    public class ConventionIdFactoryTests
    {
        [Theory]
        [InlineData("prefix", "stream", "postfix", "prefix_stream_postfix")]
        [InlineData("", "stream", "postfix", "stream_postfix")]
        [InlineData("prefix", "stream", "", "prefix_stream")]
        [InlineData("", "stream", "", "stream")]
        public void GetTopicId(string prefix, string stream, string postfix, string expectedTopic)
        {
            //Arrange
            var conventionIdFactory = new ConventionIdFactory(prefix, postfix, null);

            //Act
            var topic = conventionIdFactory.GetTopicId(stream);

            //Assert
            Assert.Equal(expectedTopic, topic);
        }
        
        [Theory]
        [InlineData("prefix","subscriptionIdentifier", "stream", "postfix", "prefix_subscriptionIdentifier_stream_postfix")]
        [InlineData("", "subscriptionIdentifier", "stream", "postfix", "subscriptionIdentifier_stream_postfix")]
        [InlineData("prefix", "subscriptionIdentifier", "stream", "", "prefix_subscriptionIdentifier_stream")]
        [InlineData("", "subscriptionIdentifier", "stream", "", "subscriptionIdentifier_stream")]
        public void GetSubscriptionId(string prefix, string subscriptionIdentifer, string stream, string postfix, string expectedTopic)
        {
            //Arrange
            var conventionIdFactory = new ConventionIdFactory(prefix, postfix, subscriptionIdentifer);

            //Act
            var topic = conventionIdFactory.GetSubscriptionId(stream);

            //Assert
            Assert.Equal(expectedTopic, topic);
        }
    }
}
