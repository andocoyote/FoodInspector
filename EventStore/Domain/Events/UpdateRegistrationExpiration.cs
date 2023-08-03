namespace EventStore.Domain.Events
{
    using EventStore.Events;
    using System;

    public class UpdateRegistrationExpiration : Event<TestEvent>
    {
        private const string NewExpirationDateKey = "NewExpirationDate";
        public DateTime NewExpirationDate()
        {
            return DateTime.Parse(this.Data[NewExpirationDateKey]);
        }

        public UpdateRegistrationExpiration() : base()
        { }

        public UpdateRegistrationExpiration(Event<TestEvent> genericEvent) :
            base(genericEvent.id, genericEvent.StreamId, genericEvent.EventNumber, new Version(genericEvent.Version), genericEvent.Created, nameof(UpdateRegistrationExpiration), genericEvent.OriginatingComponent, new Version(genericEvent.Version))
        {
            this.Data[NewExpirationDateKey] = genericEvent.Data[NewExpirationDateKey];
        }

        public UpdateRegistrationExpiration(Guid id, Guid streamId, long eventNumber, Version version, DateTime created, DateTime newExpirationDate, string originatingComponent, Version originatingComponentVersion) :
            base(id, streamId, eventNumber, version, created, nameof(UpdateRegistrationExpiration), originatingComponent, originatingComponentVersion)
        {
            this.Data[NewExpirationDateKey] = newExpirationDate.ToString();
        }

        public override TestEvent ApplyEvent(TestEvent model)
        {
            return new TestEvent(model.id, model.Owner, model.Longnumber, DateTime.Parse(this.Data[NewExpirationDateKey]));
        }
    }
}
