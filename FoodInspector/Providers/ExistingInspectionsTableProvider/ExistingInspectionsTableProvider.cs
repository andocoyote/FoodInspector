using Azure;
using Azure.Data.Tables;
using CommonFunctionality.StorageAccount;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodInspector.Providers.ExistingInspectionsTableProvider
{
    public class ExistingInspectionsTableProvider : IExistingInspectionsTableProvider
    {
        private string _tablename = "ExistingInspections";
        private string _tableStorageUri = "https://stfoodinspector.table.core.windows.net";
        private string _tableStorageAccountName = "stfoodinspector";

        private TableServiceClient _tableServiceClient = null;
        private TableClient _tableClient = null;

        private readonly IOptions<StorageAccountOptions> _storageAccountOptions;
        private readonly ILogger _logger;

        public ExistingInspectionsTableProvider(
            IOptions<StorageAccountOptions> storageAccountOptions,
            ILoggerFactory loggerFactory)
        {
            _storageAccountOptions = storageAccountOptions;
            _logger = loggerFactory.CreateLogger<ExistingInspectionsTableProvider>();

            // Create the table of establishments if it doesn't exist
            this.CreateTableClientAsync().GetAwaiter().GetResult();
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

        /// <summary>
        /// Adds a record to Azure Storage Table to identify a particular inspection result
        /// </summary>
        /// <param name="inspectionSerialNum">The serial number of the inspection</param>
        /// <param name="inspectionId">The ID for the specific entry in the inspection</param>
        /// <returns></returns>
        public async Task AddInspectionRecord(string inspectionSerialNum, string inspectionId)
        {
            Dictionary<string, object> record = new Dictionary<string, object>()
            {
                ["PartitionKey"] = inspectionSerialNum,
                ["RowKey"] = inspectionId
            };

            TableEntity entity = new TableEntity(record);

            await _tableClient.UpsertEntityAsync(entity);
        }

        /// <summary>
        /// Query Azure Storage Table for records from the specific inspection
        /// </summary>
        //// <param name="inspectionSerialNum">The serial number of the inspection</param>
        /// <param name="inspectionId">The ID for the specific entry in the inspection</param>
        /// <returns></returns>
        public async Task<List<ExistingInspectionModel>> QueryInspectionRecord(string inspectionSerialNum, string inspectionId)
        {
            // https://briancaos.wordpress.com/2022/11/11/c-azure-table-storage-queryasync-paging-and-filtering/
            AsyncPageable<ExistingInspectionModel> inspections = _tableClient.QueryAsync<ExistingInspectionModel>
                (x => x.PartitionKey == inspectionSerialNum && x.RowKey == inspectionId);

            List<ExistingInspectionModel> existingInspectionModelsList = new List<ExistingInspectionModel>();

            // Each item must be awaited in order to retrieve it
            await foreach (ExistingInspectionModel inspection in inspections)
            {
                existingInspectionModelsList.Add(inspection);
            }

            return existingInspectionModelsList;
        }
    }
}
