using MongoDB.Bson;

namespace EverStore.Domain
{
    public class Version
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public long Number { get; set; }
    }
}