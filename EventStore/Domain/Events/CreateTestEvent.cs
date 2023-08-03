using EventStore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace EventStore.Domain.Events
{
    public  class CreateTestEvent : Event<TestEvent>
    {
        private const string CreateTestEventOwnerKey = "CreateTestEventOwner";

        public string Owner()
        {
            return this.Data[CreateTestEventOwnerKey];
        }

        public CreateTestEvent() : base()
        { }

        public CreateTestEvent(Event<TestEvent> genericEvent) :
            base(genericEvent.id, genericEvent.StreamId, genericEvent.EventNumber, new Version(genericEvent.Version), genericEvent.Created, nameof(CreateTestEvent), genericEvent.OriginatingComponent, new Version(genericEvent.Version))
        {
            this.Data[CreateTestEventOwnerKey] = genericEvent.Data[CreateTestEventOwnerKey];
        }

        public CreateTestEvent(Guid id, Guid streamId, long eventNumber, Version version, DateTime created, string newOwner, string originatingComponent, Version originatingComponentVersion) :
            base(id, streamId, eventNumber, version, created, nameof(CreateTestEvent), originatingComponent, originatingComponentVersion)
        {
            this.Data[CreateTestEventOwnerKey] = newOwner;
        }

        public override TestEvent ApplyEvent(TestEvent model)
        {
            return new TestEvent(this.StreamId, Owner(), model.Longnumber, model.SomeDateTime);
        }
    }
}
