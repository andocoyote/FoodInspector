namespace EventStore.Domain;
public interface IDomainModel
{
    string StreamId { get; init; }
    bool IsFailed { get; }
    bool IsCancelled { get; }
    bool IsSucceeded { get; }

}
