using System;
using System.Linq;
using EverStore.Domain;
using MongoDB.Driver;

namespace EverStore.Contract
{
    public class ReadEvents: IDisposable
    {
        private IAsyncCursor<PersistedEvent> _persistedEventCursor;

        internal ReadEvents(IAsyncCursor<PersistedEvent> persistedEventCursor)
        {
            _persistedEventCursor = persistedEventCursor;
        }

        public IOrderedEnumerable<ResolvedEvent> Events => _persistedEventCursor.Current.Select(e => e.ToDto()).OrderBy(e => e.GlobalVersion);

        public bool MoveNext()
        {
            return _persistedEventCursor.MoveNext();
        }

        public void Dispose()
        {
            _persistedEventCursor?.Dispose();
            _persistedEventCursor = null;
        }
    }
}