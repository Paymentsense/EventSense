using Xunit;

namespace EverStore.Integration.Tests
{
    [CollectionDefinition("MongoCollection")]
    public class MongoCollection : ICollectionFixture<MongoFixture>
    {
    // This class has no code, and is never created.Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
    }
}