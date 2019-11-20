using System;
using EverStore.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace EverStore
{
    public class MongoContext : IMongoContext
    {
        private IMongoClient Client { get; set; }

        private IMongoDatabase Database { get; set; }

        private MongoContext()
        {
        }

        public IMongoCollection<T> Collection<T>()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }

        public static MongoContext Create(string eventStorageName, IMongoClient mongoClient)
        {
            BsonSerializer.RegisterSerializer(typeof(DateTimeOffset), (IBsonSerializer) new DateTimeOffsetSerializer(BsonType.String));

            var mongoContext = new MongoContext
            {
                Client = mongoClient
            };

            mongoContext.Database = mongoContext.Client.GetDatabase(eventStorageName);

            var globalVersionIndexKey = Builders<PersistedEvent>.IndexKeys.Ascending(_ => _.GlobalVersion);
            var createGlobalVersionIndexOptions = new CreateIndexOptions
            {
                Unique = true,
                Sparse = false,
                Name = $"{typeof(PersistedEvent).Name}_UniqueIndex_{nameof(PersistedEvent.GlobalVersion)}"
            };
            var globalVersionIndexModel = new CreateIndexModel<PersistedEvent>(globalVersionIndexKey, createGlobalVersionIndexOptions);
            mongoContext.Collection<PersistedEvent>().Indexes.CreateOneAsync(globalVersionIndexModel).GetAwaiter().GetResult();

            var streamVersionIndexKey = Builders<PersistedEvent>.IndexKeys.Combine(Builders<PersistedEvent>.IndexKeys.Text(_ => _.Stream), Builders<PersistedEvent>.IndexKeys.Ascending(_ =>_.SreamVersion));
            var createStreamVersionIndexOptions = new CreateIndexOptions
            {
                Unique = true,
                Sparse = false,
                Name = $"{typeof(PersistedEvent).Name}_UniqueCompositeIndex_{nameof(PersistedEvent.Stream)}_{nameof(PersistedEvent.SreamVersion)}"
            };
            var streamVersionIndexModel = new CreateIndexModel<PersistedEvent>(streamVersionIndexKey, createStreamVersionIndexOptions);
            mongoContext.Collection<PersistedEvent>().Indexes.CreateOneAsync(streamVersionIndexModel).GetAwaiter().GetResult();

            return mongoContext;
        }

    }
}