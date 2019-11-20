using System;
using EverStore.Storage;
using Mongo2Go;
using MongoDB.Driver;

namespace EverStore.Integration.Tests
{
    public class MongoFixture : IDisposable
    {
        private const string _eventStorageName = "TestEventStorage";
        private readonly MongoDbRunner _mongoRunner;

        public MongoFixture()
        {
            _mongoRunner = MongoDbRunner.Start();
            MongoClient = new MongoClient(_mongoRunner.ConnectionString);
            EverStore.Storage.MongoContext.RegisterSerializerOptions();
        }

        public void SetupFixture()
        {
            var database = MongoClient.GetDatabase(_eventStorageName);
            ResetDatabase(database);
            MongoContext = EverStore.Storage.MongoContext.CreateAsync(_eventStorageName, MongoClient).GetAwaiter().GetResult();
        }
        
        internal IMongoContext MongoContext { get; private set; }
        public string DatabaseName => _eventStorageName;
        public IMongoClient MongoClient { get; }

        public void Dispose()
        {
            _mongoRunner.Dispose();
        }

        private void ResetDatabase(IMongoDatabase database)
        {
            foreach (var collectionName in database.ListCollectionNames().ToList())
            {
                database.DropCollection(collectionName);
            }
        }
    }
}