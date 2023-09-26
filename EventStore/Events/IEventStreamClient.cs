namespace EventStore.Events
{
    using EventStore.Domain;
    using System;
    using System.Threading.Tasks;

    public interface IEventStreamClient<DomainModel> where DomainModel : IDomainModel
    {
        Task<IEventStream<DomainModel>> CreateEventStream();
        Task<IEventStream<DomainModel>> GetEventStream(Guid streamId);
    }
}
