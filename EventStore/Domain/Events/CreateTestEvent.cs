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
            //return this.Data[CreateTestEventOwnerKey];
            return string.Empty;
        }

        public CreateTestEvent() : base()
        { }

        public CreateTestEvent(Event<TestEvent> genericEvent) :
            base(genericEvent.id, genericEvent.StreamId, genericEvent.EventNumber, genericEvent.Created, nameof(CreateTestEvent), genericEvent.OriginatingComponent, genericEvent.OriginatingComponentVersion)
        {
            //this.Data[CreateTestEventOwnerKey] = genericEvent.Data[CreateTestEventOwnerKey];
        }

        public CreateTestEvent(string id, string streamId, long eventNumber, DateTime created, string newOwner, string originatingComponent, string originatingComponentVersion) :
            base(id, streamId, eventNumber, created, nameof(CreateTestEvent), originatingComponent, originatingComponentVersion)
        {
            //this.Data[CreateTestEventOwnerKey] = newOwner;
        }

        public override TestEvent ApplyEvent(TestEvent model)
        {
            return new TestEvent(this.StreamId, Owner(), model.Longnumber, model.SomeDateTime);
        }
    }
}
