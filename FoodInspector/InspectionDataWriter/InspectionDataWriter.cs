using FoodInspector.KeyVaultProvider;
using FoodInspector.Model;
using FoodInspector.SQLDatabaseProvider;
using FoodInspector.StorageTableProvider;
using HttpClientTest.HttpHelpers;
using Microsoft.Extensions.Logging;

namespace FoodInspector.InspectionDataWriter
{
    public class InspectionDataWriter : IInspectionDataWriter
    {
        private readonly ICommonServiceLayerProvider _commonServiceLayerProvider;
        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ISQLDatabaseProvider _sqlDatabaseProvider;
        private readonly IStorageTableProvider _storageTableProvider;
        private readonly ILogger _logger;

        public InspectionDataWriter(
            ICommonServiceLayerProvider commonServiceLayerProvider,
            IKeyVaultProvider keyVaultProvider,
            ISQLDatabaseProvider sqlDatabaseProvider,
            IStorageTableProvider storageTableProvider,
            ILoggerFactory loggerFactory)
        {
            _commonServiceLayerProvider = commonServiceLayerProvider;
            _keyVaultProvider = keyVaultProvider;
            _sqlDatabaseProvider = sqlDatabaseProvider;
            _storageTableProvider = storageTableProvider;
            _logger = loggerFactory.CreateLogger<InspectionDataWriter>();
        }

        public async Task WriteData()
        {
            try
            {
                _logger.LogInformation("Upsert called.");

                //List<InspectionData> inspectionData = await _commonServiceLayerProvider.GetInspections("", "Redmond", "2022-01-01");

                //_logger.LogInformation($"{inspectionData.Count} records found.");

                //_sqlDatabaseProvider.WriteRecord(inspectionData[0]);
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"[UpsertData] An exception was caught: {ex}");
            }
        }
    }
}
