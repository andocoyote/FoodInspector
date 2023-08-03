namespace EventStore.Domain.Events
{
    using EventStore.Domain;
    using EventStore.Events;
    using System;

    public interface ITestEventFactory
    {
        CreateTestEvent BuildCreateTestEvent(
            Event<TestEvent> genericEvent);

        CreateTestEvent BuildCreateTestEvent(
            IEventStream<TestEvent> eventStream,
            string newOwner,
            string originatingComponent,
            Version originatingComponentVersion);

        UpdateRegistrationExpiration BuildUpdateRegistrationExpiration(
            Event<TestEvent> genericEvent);

        UpdateRegistrationExpiration BuildUpdateRegistrationExpiration(
            IEventStream<TestEvent> eventStream,
            DateTime newExpirationDate,
            string originatingComponent,
            Version originatingComponentVersion);
    }
}
