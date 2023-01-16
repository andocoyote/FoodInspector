using CommonFunctionality.KeyVaultProvider;
using FoodInspector.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace FoodInspector.CosmosDbProvider
{
    public class CosmosDbProvider : ICosmosDbProvider
    {
        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ILogger _logger;
        private CosmosClient _client = null;

        private string _database = "FoodInspector";
        private string _container = "InspectionData";

        public CosmosDbProvider(
            IKeyVaultProvider keyVaultProvider,
            ILoggerFactory loggerFactory)
        {
            _keyVaultProvider = keyVaultProvider;
            _logger = loggerFactory.CreateLogger<CosmosDbProvider>();

            string cosmosDbConnectionString = _keyVaultProvider.GetKeyVaultSecret(KeyVaultSecretNames.cosmosfoodinspectorConnectionString).GetAwaiter().GetResult();

            // Cosmos DB client is intended to be instantiated once per application and reused
            _client = new CosmosClient(cosmosDbConnectionString);
        }

        /// <summary>
        /// Write the InspectionData object to the Cosmos DB container
        /// </summary>
        /// <param name="inspectionData">The InspectionData object to write</param>
        /// <returns></returns>
        public async Task WriteDocument(InspectionData inspectionData)
        {
            await _client.GetContainer(_database, _container).UpsertItemAsync(inspectionData);
        }
    }
}
