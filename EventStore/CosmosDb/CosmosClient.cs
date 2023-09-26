using Azure.Identity;
using CommonFunctionality.CosmosDbProvider;
using EventStore.Domain;
using EventStore.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace EventStore.CosmosDb
{
    public class CosmosClient<DomainModel> : ICosmosClient<DomainModel> where DomainModel : IDomainModel
    {
        private Container cosmosContainer;

        public CosmosClient() { }

        public CosmosClient(
            IOptions<CosmosClientOptions> options)
        {
            CosmosClient cosmosClient = new CosmosClient(
                options.Value.AccountEndpoint,
                new DefaultAzureCredential());

            //Database db = cosmosClient.CreateDatabaseAsync(options.Value.Database).GetAwaiter().GetResult();
            //Container container = db.CreateContainerAsync(options.Value.Containers.TestEvents, "id").GetAwaiter().GetResult();

            Database db = cosmosClient.GetDatabase(options.Value.Database);
            Container container = db.GetContainer(options.Value.Containers.TestEvents);

            this.cosmosContainer = container;
        }

        public async Task WriteEvent(Event<DomainModel> @event)
        {
            await this.cosmosContainer.CreateItemAsync(@event);
        }
    }
}
