namespace EverStore.Storage
{
    internal interface IVersionRepository
    {
        long GetNextGlobalVersion();
    }
}