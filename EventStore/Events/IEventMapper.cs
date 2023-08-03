namespace EventStore.Events
{
    using System.Threading.Tasks;

    public interface IEventMapper<DomainModel> where DomainModel : new()
    {
        Event<DomainModel> MapToEventImplementation(Event<DomainModel> genericEvent);
    }
}
