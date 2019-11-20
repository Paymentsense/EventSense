using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace EverStore.Tests
{
    public class EventContextTests
    {
        [Fact]
        public void CreatesContext()
        {
            var mongoClient = Substitute.For<IMongoClient>();
            var context = EventContext.Create("EventStorage", mongoClient);

            Assert.NotNull(context);
        }
    }
}
