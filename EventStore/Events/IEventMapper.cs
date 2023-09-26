namespace EventStore.Events
{
    using EventStore.Domain;
    using System.Threading.Tasks;

    public interface IEventMapper<DomainModel> where DomainModel : IDomainModel
    {
        Event<DomainModel> MapToEventImplementation(Event<DomainModel> genericEvent);
    }
}
