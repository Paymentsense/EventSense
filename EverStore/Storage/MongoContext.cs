using System;
using System.Threading.Tasks;
using EverStore.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Version = EverStore.Domain.Version;

namespace EverStore.Storage
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

        public static void RegisterSerializerOptions()
        {
            BsonSerializer.RegisterSerializer(typeof(DateTimeOffset), (IBsonSerializer)new DateTimeOffsetSerializer(BsonType.String));
        }

        public static async Task<MongoContext> CreateAsync(string eventStorageName, IMongoClient mongoClient)
        {
            var mongoContext = new MongoContext
            {
                Client = mongoClient
            };

            mongoContext.Database = mongoContext.Client.GetDatabase(eventStorageName);

            var streamVersionIndexKey = Builders<PersistedEvent>.IndexKeys.Combine(Builders<PersistedEvent>.IndexKeys.Text(_ => _.Stream), Builders<PersistedEvent>.IndexKeys.Ascending(_ =>_.StreamVersion));
            var createStreamVersionIndexOptions = new CreateIndexOptions
            {
                Unique = true,
                Sparse = false,
                Name = $"{typeof(PersistedEvent).Name}_UniqueCompositeIndex_{nameof(PersistedEvent.Stream)}_{nameof(PersistedEvent.StreamVersion)}"
            };
            var streamVersionIndexModel = new CreateIndexModel<PersistedEvent>(streamVersionIndexKey, createStreamVersionIndexOptions);
            await mongoContext.Collection<PersistedEvent>().Indexes.CreateOneAsync(streamVersionIndexModel);

            var versionIndexKey = Builders<Version>.IndexKeys.Ascending(_ => _.Name);
            var createVersionIndexOptions = new CreateIndexOptions
            {
                Unique = true,
                Sparse = false,
                Name = $"{typeof(Version).Name}_UniqueIndex_{nameof(Version.Name)}"
            };
            var versionIndexModel = new CreateIndexModel<Version>(versionIndexKey, createVersionIndexOptions);
            await mongoContext.Collection<Version>().Indexes.CreateOneAsync(versionIndexModel);

            return mongoContext;
        }

    }
}