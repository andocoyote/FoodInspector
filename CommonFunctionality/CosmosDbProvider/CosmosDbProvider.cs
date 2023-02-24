using CommonFunctionality.KeyVaultProvider;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace CommonFunctionality.CosmosDbProvider
{
    public class CosmosDbProviderBase
    {
        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ILogger _logger;
        private static CosmosClient _client = null;
        public string Database { get; set; } = null;
        private string Container { get; set; } = null;

        public CosmosDbProviderBase(
            IKeyVaultProvider keyVaultProvider,
            ILoggerFactory loggerFactory,
            string database,
            string container,
            string connectionString)
        {
            _keyVaultProvider = keyVaultProvider;
            _logger = loggerFactory.CreateLogger<CosmosDbProviderBase>();
            Database = database;
            Container = container;

            string cosmosDbConnectionString = connectionString;

            // Cosmos DB client is intended to be instantiated once per application and reused
            _client = new CosmosClient(cosmosDbConnectionString);
        }

        /// <summary>
        /// Write the InspectionData object to the Cosmos DB container
        /// </summary>
        /// <param name="inspectionData">The InspectionData object to write</param>
        /// <returns></returns>
        protected async Task WriteDocument<T>(T document)
        {
            await _client.GetContainer(Database, Container).UpsertItemAsync(document);
        }

        /// <summary>
        /// Read the InspectionData object from the Cosmos DB container
        /// </summary>
        /// <param name="id">The ID of the document to read</param>
        /// <param name="partitionKey">The partition key of the document to read</param>
        /// <returns>The document read from Cosmos DB</returns>
        protected async Task<T> ReadDocument<T>(string id, PartitionKey partitionKey)
        {
            T doc = await _client.GetContainer(Database, Container).ReadItemAsync<T>(id, partitionKey);

            return doc;
        }
    }
}
