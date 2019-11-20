using MongoDB.Driver;
using NSubstitute;
using OpenTracing.Mock;
using Xunit;

namespace EverStore.Tests
{
    public class EventContextTests
    {
        [Fact]
        public void CreatesContext()
        {
            var mongoClient = Substitute.For<IMongoClient>();
            var context = EventContext.Create("projectId", "EventStorage", mongoClient, new MockTracer());

            Assert.NotNull(context);
        }
    }
}
