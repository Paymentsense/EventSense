using System;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace EverStore.Integration.Tests
{
    public class MongoContextTests: IDisposable
    {
        private MongoDbRunner _mongoDbRunner;

        public MongoContextTests()
        {
            _mongoDbRunner = MongoDbRunner.Start();
        }

        [Fact]
        public void ConstructsIndexes()
        {
            var context = MongoContext.Create("TestEventStorage", new MongoClient(_mongoDbRunner.ConnectionString));
            Assert.NotNull(context);
        }

        public void Dispose()
        {
            _mongoDbRunner.Dispose();
        }
    }
}
