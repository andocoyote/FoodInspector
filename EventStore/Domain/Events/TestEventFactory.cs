namespace EventStore.Domain.Events
{
    using System;
    using EventStore.Events;

    public class TestEventFactory : ITestEventFactory
    {
        public CreateTestEvent BuildCreateTestEvent(Event<TestEvent> genericEvent)
        {
            return new CreateTestEvent(genericEvent);
        }

        public CreateTestEvent BuildCreateTestEvent(
           IEventStream<TestEvent> eventStream,
           string newOwner,
           string originatingComponent,
           Version originatingComponentVersion)
        {
            return new CreateTestEvent(
                Guid.NewGuid().ToString(),
                eventStream.GetStreamId().ToString(),
                eventStream.NextEventNumber(),
                DateTime.UtcNow,
                newOwner,
                originatingComponent,
                originatingComponentVersion.ToString());
        }

        public UpdateRegistrationExpiration BuildUpdateRegistrationExpiration(Event<TestEvent> genericEvent)
        {
            return new UpdateRegistrationExpiration(genericEvent);
        }

        public UpdateRegistrationExpiration BuildUpdateRegistrationExpiration(
            IEventStream<TestEvent> eventStream,
            DateTime newExpirationDate,
            string originatingComponent,
            Version originatingComponentVersion)
        {
            return new UpdateRegistrationExpiration(
                Guid.NewGuid().ToString(),
                eventStream.GetStreamId().ToString(),
                eventStream.NextEventNumber(),
                DateTime.UtcNow, newExpirationDate,
                originatingComponent,
                originatingComponentVersion.ToString());
        }
    }
}
