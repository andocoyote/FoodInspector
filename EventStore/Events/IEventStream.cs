namespace EventStore.Events
{
    using EventStore.Domain;
    using System;
    using System.Threading.Tasks;

    public interface IEventStream<DomainModel> where DomainModel : IDomainModel
    {
        Task AppendEvent<EventType>(EventType newEvent) where EventType : Event<DomainModel>;
        DomainModel ProjectModel();
        Guid GetStreamId();
        long NextEventNumber();
        Version CurrentVersion();
    }
}
