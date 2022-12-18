using Azure.Data.Tables;
using FoodInspector.KeyVaultProvider;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure;
using Azure.Data.Tables.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodInspector.StorageTableProvider
{
    public class StorageTableProvider : IStorageTableProvider
    {
        private string _tablename = "FoodInspectorEstablishments";
        private string _tableStorageUri = "https://stfoodinspector.table.core.windows.net";
        private string _tableStorageAccountName = "stfoodinspector";

        private TableServiceClient _tableServiceClient = null;
        private TableClient _tableClient = null;

        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ILogger _logger;

        public StorageTableProvider(
            IKeyVaultProvider keyVaultProvider,
            ILoggerFactory loggerFactory)
        {
            _keyVaultProvider = keyVaultProvider;
            _logger = loggerFactory.CreateLogger<StorageTableProvider>();

            // Create the table of establishments if it doesn't exist
            this.CreateTableClientAsync().GetAwaiter().GetResult();
        }

        private async Task CreateTableClientAsync()
        {
            try
            {
                string storageKey = await _keyVaultProvider.GetKeyVaultSecret(KeyVaultSecretNames.stfoodinspectorKey);

                _tableServiceClient = new TableServiceClient(
                    new Uri(_tableStorageUri),
                    new TableSharedKeyCredential(_tableStorageAccountName, storageKey));

                await _tableServiceClient.CreateTableIfNotExistsAsync(_tablename);

                _tableClient = _tableServiceClient.GetTableClient(_tablename);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CreateTableIfNotExistsAsync] Exception caught: {ex}");
            }
        }

        // Table operations:
        //  Use TableServiceClient: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme?view=azure-dotnet
        //  Create the table
        //  Update the table with Establishments.json
        //  Read all records from the table
        //  Define the interface in such a way that it could easily be swapped out for another component

        // Inspections service:
        //  iterate over each table record object and call the Food Inspections API
        //  write to SQL
        public async Task CreateEstablishmentsSet()
        {
            string path = Environment.CurrentDirectory + @"\StorageTableProvider\Establishments.json";

            string json = File.ReadAllText(path);
            List<EstablishmentsModel> establishments = JsonConvert.DeserializeObject<List<EstablishmentsModel>>(json);

            foreach (EstablishmentsModel establishment in establishments)
            {
                _logger.LogInformation(
                    "[CreateEstablishmentsSet]: " +
                    $"PartitionKey: {establishment.PartitionKey} " +
                    $"RowKey: {establishment.RowKey} " +
                    $"Name: {establishment.Name} " +
                    $"City: {establishment.City}");

                Dictionary<string, object> record = new Dictionary<string, object>()
                {
                    ["PartitionKey"] = establishment.PartitionKey,
                    ["RowKey"] = establishment.RowKey,
                    ["Name"] = establishment.Name,
                    ["City"] = establishment.City
                };

                TableEntity entity = new TableEntity(record);

                await _tableClient.UpsertEntityAsync(entity);
            }
        }

        public async Task<List<EstablishmentsModel>> GetEstablishmentsSet()
        {
            List<EstablishmentsModel> establishmentsList = new List<EstablishmentsModel>();
            var establishments = _tableClient.QueryAsync<EstablishmentsModel>(filter: "");
            await foreach (EstablishmentsModel establishment in establishments)
            {
                establishmentsList.Add(establishment);
            }
            return establishmentsList;
        }
    }
}
