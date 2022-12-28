using FoodInspector.EstablishmentsProvider;
using FoodInspector.KeyVaultProvider;
using FoodInspector.Model;
using FoodInspector.SQLDatabaseProvider;
using FoodInspector.EstablishmentsTableProvider;
using HttpClientTest.HttpHelpers;
using Microsoft.Extensions.Logging;

namespace FoodInspector.InspectionDataGatherer
{
    public class InspectionDataGatherer : IInspectionDataGatherer
    {
        private readonly ICommonServiceLayerProvider _commonServiceLayerProvider;
        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ISQLDatabaseProvider _sqlDatabaseProvider;
        private readonly IEstablishmentsTableProvider _storageTableProvider;
        private readonly ILogger _logger;

        public InspectionDataGatherer(
            ICommonServiceLayerProvider commonServiceLayerProvider,
            IKeyVaultProvider keyVaultProvider,
            ISQLDatabaseProvider sqlDatabaseProvider,
            IEstablishmentsTableProvider storageTableProvider,
            ILoggerFactory loggerFactory)
        {
            _commonServiceLayerProvider = commonServiceLayerProvider;
            _keyVaultProvider = keyVaultProvider;
            _sqlDatabaseProvider = sqlDatabaseProvider;
            _storageTableProvider = storageTableProvider;
            _logger = loggerFactory.CreateLogger<InspectionDataGatherer>();
        }

        public async Task<List<InspectionData>> GatherData()
        {
            List<InspectionData> inspectionData = null;

            // Populate the table of establishments from the text file
            await _storageTableProvider.CreateEstablishmentsSet();

            // Read the set of establishment properties from the table to query
            List<EstablishmentsModel> establishmentsList = await _storageTableProvider.GetEstablishmentsSet();

            foreach (EstablishmentsModel establishment in establishmentsList)
            {
                _logger.LogInformation(
                    "[GatherData]: " +
                    $"PartitionKey: {establishment.PartitionKey} " +
                    $"RowKey: {establishment.RowKey} " +
                    $"Name: {establishment.Name} " +
                    $"City: {establishment.City}");
            }

            // Query the API to obtain the food inspection records
            inspectionData = await _commonServiceLayerProvider.GetInspections(establishmentsList);

            return inspectionData;
        }
    }
}
