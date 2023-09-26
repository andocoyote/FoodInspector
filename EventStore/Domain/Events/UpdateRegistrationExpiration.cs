namespace EventStore.Domain.Events
{
    using EventStore.Events;
    using System;

    public class UpdateRegistrationExpiration : Event<TestEvent>
    {
        private const string NewExpirationDateKey = "NewExpirationDate";
        public DateTime NewExpirationDate()
        {
            return DateTime.UtcNow;
        }

        public UpdateRegistrationExpiration() : base()
        { }

        public UpdateRegistrationExpiration(Event<TestEvent> genericEvent) :
            base(genericEvent.id, genericEvent.StreamId, genericEvent.EventNumber, genericEvent.Created, nameof(UpdateRegistrationExpiration), genericEvent.OriginatingComponent, genericEvent.OriginatingComponentVersion)
        {
            //this.Data[NewExpirationDateKey] = genericEvent.Data[NewExpirationDateKey];
        }

        public UpdateRegistrationExpiration(string id, string streamId, long eventNumber, DateTime created, DateTime newExpirationDate, string originatingComponent, string originatingComponentVersion) :
            base(id, streamId, eventNumber, created, nameof(UpdateRegistrationExpiration), originatingComponent, originatingComponentVersion)
        {
            //this.Data[NewExpirationDateKey] = newExpirationDate.ToString();
        }

        public override TestEvent ApplyEvent(TestEvent model)
        {
            return new TestEvent(model.id, model.Owner, model.Longnumber, DateTime.UtcNow);
        }
    }
}
