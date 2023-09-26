using EventStore.Domain;
using EventStore.Events;

namespace EventStore.CosmosDb
{
    public interface ICosmosClient<DomainModel> where DomainModel : IDomainModel
    {
        Task WriteEvent(Event<DomainModel> @event);
    }
}
