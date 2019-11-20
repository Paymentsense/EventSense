using EverStore.Storage;
using MongoDB.Driver;
using Xunit;

namespace EverStore.Integration.Tests.Storage
{
    [Collection("MongoCollection")]

    public class VersionRepositoryTests
    {
        private MongoFixture _mongoFixture;

        public VersionRepositoryTests(MongoFixture mongoFixture)
        {
            _mongoFixture = mongoFixture;
            mongoFixture.SetupFixture();
        }

        [Fact]
        public void GetNextGlobalVersion_AreSequential()
        {
            var repo = new VersionRepository(_mongoFixture.MongoContext);

            var firstVersion = repo.GetNextGlobalVersion();
            var secondVersion = repo.GetNextGlobalVersion();

            Assert.NotEqual(firstVersion, secondVersion);
            Assert.Equal(firstVersion + 1, secondVersion);
        }
    }
}
