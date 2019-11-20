using System.Threading.Tasks;
using EverStore.Model;

namespace EverStore.Messaging
{
    internal interface IEventStreamPublisher
    {
        Task<string> Publish(PersistedEvent @event, string stream, string streamAggregate, string streamId);
    }
}