using EventStore.Events;

namespace EventStore.CosmosDb
{
    public interface ICosmosClient<DomainModel> where DomainModel : new()
    {
        Task WriteEvent(Event<DomainModel> @event);
    }
}
