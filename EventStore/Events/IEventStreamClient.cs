namespace EventStore.Events
{
    using System;
    using System.Threading.Tasks;

    public interface IEventStreamClient<DomainModel> where DomainModel : new()
    {
        Task<IEventStream<DomainModel>> CreateEventStream();
        Task<IEventStream<DomainModel>> GetEventStream(Guid streamId);
    }
}
