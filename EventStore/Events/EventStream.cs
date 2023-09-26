namespace EventStore.Events
{
    using EventStore.Domain;
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class EventStream<DomainModel> : IEventStream<DomainModel> where DomainModel : IDomainModel, new()
    {
        private IEnumerable<Event<DomainModel>> events;
        private Container cosmosContainer;
        private Guid streamId;
        private IEventMapper<DomainModel> eventMapper;

        public EventStream(Guid streamId, Container cosmosContainer, IEnumerable<Event<DomainModel>> events, IEventMapper<DomainModel> eventMapper)
        {
            this.cosmosContainer = cosmosContainer;
            this.events = events;
            this.streamId = streamId;
            this.eventMapper = eventMapper;
        }

        public async Task AppendEvent<EventType>(EventType newEvent) where EventType : Event<DomainModel>
        {
            // Getting current model
            var currentModel = this.ProjectModel();

            // Checking if event can actually be applied.
            newEvent.ApplyEvent(currentModel);

            // If applied adding event to stream
            await this.cosmosContainer.CreateItemAsync(newEvent);

            // Updating local copy
            this.events = this.events.Concat(new List<Event<DomainModel>> { newEvent });
        }

        public Guid GetStreamId()
        {
            return this.streamId;
        }

        public Version CurrentVersion()
        {
            return new Version("0.0.0.1");
        }

        public long NextEventNumber()
        {
            if (!events.Any())
            {
                return (long)1;
            }
            else
            {
                return events.Last().EventNumber + 1;
            }
        }

        public DomainModel ProjectModel()
        {
            var model = new DomainModel();
            var mappedEventTasks = events.Select(e => this.eventMapper.MapToEventImplementation(e));
            var mappedEvents = mappedEventTasks;
            return mappedEvents.Aggregate(model, (currentModel, e) =>
            {
                return e.ApplyEvent(currentModel);
            });
        }
    }
}
