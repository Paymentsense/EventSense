using MongoDB.Driver;
using Version = EverStore.Domain.Version;

namespace EverStore.Storage
{
    internal class VersionRepository: IVersionRepository
    {
        private const string GlobalVersion = "Version";
        private readonly IMongoContext _mongoContext;

        public VersionRepository(IMongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public long GetNextGlobalVersion()
        {
            var collection = _mongoContext.Collection<Domain.Version>();
            var update = Builders<Version>.Update.Inc(v => v.Number, 1);
            var filter = Builders<Version>.Filter.Eq(counter => counter.Name, GlobalVersion);
            var options = new FindOneAndUpdateOptions<Version>() { IsUpsert = true, ReturnDocument = ReturnDocument.After };
            var globalVersion = collection.FindOneAndUpdate(filter, update: update, options: options);

            return globalVersion.Number;
        }
    }
}
