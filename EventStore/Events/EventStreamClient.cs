namespace EventStore.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EventStore.Domain;
    using Microsoft.Azure.Cosmos;

    public class EventStreamClient<DomainModel> : IEventStreamClient<DomainModel> where DomainModel : IDomainModel, new()
    {
        private Container cosmosContainer;
        private const string streamIdQueryFormat = "SELECT * FROM c WHERE c.StreamId = '{0}'";
        private IEventMapper<DomainModel> eventMapper;

        public EventStreamClient(Container cosmosContainer, IEventMapper<DomainModel> eventMapper)
        {
            this.cosmosContainer = cosmosContainer;
            this.eventMapper = eventMapper;
        }

        public Task<IEventStream<DomainModel>> CreateEventStream()
        {
            return Task.FromResult<IEventStream<DomainModel>>(new EventStream<DomainModel>(Guid.NewGuid(), this.cosmosContainer, new List<Event<DomainModel>>(), this.eventMapper));
        }

        public async Task<IEventStream<DomainModel>> GetEventStream(Guid streamId)
        {
            Console.WriteLine("{0} GetEventStream: streamId={1}", nameof(EventStreamClient<DomainModel>), streamId);
            var events = await GetLatestEvents(streamId);
            return new EventStream<DomainModel>(streamId, this.cosmosContainer, events, this.eventMapper);
        }

        private async Task<IEnumerable<Event<DomainModel>>> GetLatestEvents(Guid streamId)
        {
            var streamIdQuery = new QueryDefinition(string.Format(streamIdQueryFormat, streamId));
            var eventIterator = this.cosmosContainer.GetItemQueryIterator<Event<DomainModel>>(streamIdQuery);
            List<Event<DomainModel>> allStreamEvents = new List<Event<DomainModel>>();
            while (eventIterator.HasMoreResults)
            {
                FeedResponse<Event<DomainModel>> currentResultSet = await eventIterator.ReadNextAsync();
                foreach (Event<DomainModel> currentEvent in currentResultSet)
                {
                    allStreamEvents.Add(currentEvent);
                }
            }

            return allStreamEvents.OrderBy(anEvent => anEvent.EventNumber);
        }
    }
}
