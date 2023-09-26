using EventStore.Domain;

namespace FoodInspector.DomainModels
{
    public class InspectionDomainModel : IDomainModel
    {
        public string StreamId { get; init; }
        public bool IsFailed { get; }
        public bool IsCancelled { get; }
        public bool IsSucceeded { get; }

        public InspectionDomainModel() { }
    }
}
