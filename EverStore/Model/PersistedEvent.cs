namespace EverStore.Model
{
    internal class PersistedEvent
    {
        public byte[] Data { get; set; }
        public string Stream { get; set; }
        public long SreamVersion { get; set; }
        public long GlobalVersion { get; set; }
    }
}
