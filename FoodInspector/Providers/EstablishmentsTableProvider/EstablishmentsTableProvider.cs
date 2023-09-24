using Azure;
using Azure.Data.Tables;
using CommonFunctionality.StorageAccount;
using FoodInspector.Providers.EstablishmentsProvider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodInspector.Providers.EstablishmentsTableProvider
{
    public class ExistingInspectionsTableProvider : IEstablishmentsTableProvider
    {
        private string _tablename = string.Empty;
        private string _tableStorageUri = string.Empty;
        private string _tableStorageAccountName = string.Empty;

        private TableServiceClient _tableServiceClient = null;
        private TableClient _tableClient = null;

        private readonly IOptions<StorageAccountOptions> _storageAccountOptions;
        private readonly IEstablishmentsProvider _establishmentsProvider;
        private readonly ILogger _logger;

        public ExistingInspectionsTableProvider(
            IOptions<StorageAccountOptions> storageAccountOptions,
            IEstablishmentsProvider establishmentsProvider,
            ILoggerFactory loggerFactory)
        {
            _storageAccountOptions = storageAccountOptions;
            _establishmentsProvider = establishmentsProvider;
            _tablename = storageAccountOptions.Value.TableName;
            _tableStorageUri = storageAccountOptions.Value.TableEndpoint;
            _tableStorageAccountName = storageAccountOptions.Value.TableStorageAccountName;
            _logger = loggerFactory.CreateLogger<ExistingInspectionsTableProvider>();

            // Create the table of establishments if it doesn't exist
            CreateTableClientAsync().GetAwaiter().GetResult();
        }

        private async Task CreateTableClientAsync()
        {
            try
            {
                string storageKey = _storageAccountOptions.Value.StorageAccountKey;

                _tableServiceClient = new TableServiceClient(
                    new Uri(_tableStorageUri),
                    new TableSharedKeyCredential(_tableStorageAccountName, storageKey));

                await _tableServiceClient.CreateTableIfNotExistsAsync(_tablename);

                _tableClient = _tableServiceClient.GetTableClient(_tablename);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CreateTableClientAsync] Exception caught: {ex}");
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
            List<EstablishmentsModel> establishments = _establishmentsProvider.ReadEstablishmentsFile();

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

            // https://briancaos.wordpress.com/2022/11/11/c-azure-table-storage-queryasync-paging-and-filtering/
            AsyncPageable<EstablishmentsModel> establishments = _tableClient.QueryAsync<EstablishmentsModel>(filter: "");

            await foreach (EstablishmentsModel establishment in establishments)
            {
                establishmentsList.Add(establishment);
            }

            return establishmentsList;
        }
    }
}
