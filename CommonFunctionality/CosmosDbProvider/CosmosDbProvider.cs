using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommonFunctionality.CosmosDbProvider
{
    public class CosmosDbProviderBase
    {
        private readonly ILogger _logger;
        private static CosmosClient _client = null;
        private string _database { get; set; } = null;
        private string _container { get; set; } = null;

        public CosmosDbProviderBase(
            IOptions<CosmosDbOptions> cosmosDbOptions,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CosmosDbProviderBase>();
            _database = cosmosDbOptions.Value.Database;
            _container = cosmosDbOptions.Value.Containers.InspectionData;

            // Cosmos DB client is intended to be instantiated once per application and reused
            // Creating the client for use with a Managed Identity
            // Requires creation of an RBAC group with Cosmos DB access
            _client = new CosmosClient(
                cosmosDbOptions.Value.AccountEndpoint,
                new DefaultAzureCredential());
        }

        /// <summary>
        /// Write the InspectionData object to the Cosmos DB container
        /// </summary>
        /// <param name="inspectionData">The InspectionData object to write</param>
        /// <returns></returns>
        protected async Task WriteDocument<T>(T document)
        {
            await _client.GetContainer(_database, _container).UpsertItemAsync(document);
        }

        /// <summary>
        /// Read the InspectionData object from the Cosmos DB container
        /// </summary>
        /// <param name="id">The ID of the document to read</param>
        /// <param name="partitionKey">The partition key of the document to read</param>
        /// <returns>The document read from Cosmos DB</returns>
        protected async Task<T> ReadDocument<T>(string id, PartitionKey partitionKey)
        {
            T doc = await _client.GetContainer(_database, _container).ReadItemAsync<T>(id, partitionKey);

            return doc;
        }
    }
}
