using CommonFunctionality.CosmosDbProvider;
using FoodInspector.InspectionDataGatherer;
using FoodInspector.Providers.AzureAIProvider;
using FoodInspector.Providers.ExistingInspectionsTableProvider;
using FoodInspectorModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodInspector
{
    public class Functions
    {
        // https://stackoverflow.com/questions/54155903/azure-webjob-read-appsettings-json-and-inject-configuration-to-timertrigger
        private readonly IConfiguration _configuration;
        private readonly IOptions<CosmosDbOptions> _cosmosDbOptions;
        private readonly IInspectionDataGatherer _inspectionDataGatherer;
        private readonly ICosmosDbProvider<CosmosDbWriteDocument, CosmosDbReadDocument> _cosmosDbProvider;
        private readonly IExistingInspectionsTableProvider _existingInspectionsTableProvider;
        private readonly IAzureAIProvider _azureAIProvider;
        private ILogger _logger;

        public Functions(
            IConfiguration configuration,
            IOptions<CosmosDbOptions> cosmosDbOptions,
            IInspectionDataGatherer inspectionDataGatherer,
            ICosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument> cosmosDbProviderFactory,
            IExistingInspectionsTableProvider existingInspectionsTableProvider,
            IAzureAIProvider azureAIProvider)
        {
            _configuration = configuration;
            _cosmosDbOptions = cosmosDbOptions;
            _inspectionDataGatherer = inspectionDataGatherer;
            _cosmosDbProvider = cosmosDbProviderFactory.CreateProvider();
            _existingInspectionsTableProvider = existingInspectionsTableProvider;
            _azureAIProvider = azureAIProvider;
        }

        public async Task ProcessMessageOnTimer(
            [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            _logger = logger;
            _logger.LogInformation("[ProcessMessageOnTimer] TimerTrigger fired.");

            try
            {
                DisplayConfiguration();

                // Query the food inspections API for the latest data
                List<InspectionRecordAggregated> inspectionRecordAggregatedList = await _inspectionDataGatherer.QueryAllInspections();

                _logger.LogInformation($"[ProcessMessageOnTimer] inspectionRecordAggregatedList count: {(inspectionRecordAggregatedList?.Count ?? -1)}.");

                await SaveInspectionsToStorageTable(inspectionRecordAggregatedList);

                _logger.LogInformation($"[ProcessMessageOnTimer] inspectionRecordAggregatedList saved to Azure Storage.");

                await SaveInspectionsToCosmosDb(inspectionRecordAggregatedList);

                _logger.LogInformation($"[ProcessMessageOnTimer] inspectionRecordAggregatedList saved to Azure Cosmos DB.");

                List<InspectionRecordOpenAIRequestModel> inspectionRecordOpenAIRequestModels = 
                    inspectionRecordAggregatedList.Select(i => new InspectionRecordOpenAIRequestModel()
                    {
                        ProgramIdentifier = i.ProgramIdentifier,
                        InspectionScore = i.InspectionScore,
                        InspectionResult = i.InspectionResult,
                        InspectionClosedBusiness = i.InspectionClosedBusiness,
                        Violations = i.Violations,
                        InspectionSerialNum = i.InspectionSerialNum,
                    }).ToList();

                string chatResult = _azureAIProvider.Check(inspectionRecordOpenAIRequestModels);

                _logger.LogInformation(chatResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ProcessMessageOnTimer] An exception was caught. Exception: {ex}");
            }

            _logger.LogInformation("[ProcessMessageOnTimer] Processing completed.");
        }

        private void DisplayConfiguration()
        {
            _logger.LogInformation($"[ProcessMessageOnTimer] Printing App Settings via IOptions classes:");
            _logger.LogInformation($"[ProcessMessageOnTimer] _cosmosDbOptions.Value.AccountEndpoint = {_cosmosDbOptions.Value.AccountEndpoint}");
            _logger.LogInformation($"[ProcessMessageOnTimer] _cosmosDbOptions.Value.Database = {_cosmosDbOptions.Value.Database}");
            _logger.LogInformation($"[ProcessMessageOnTimer] _cosmosDbOptions.Value.Containers.InspectionData = {_cosmosDbOptions.Value.Containers.InspectionData}");

            _logger.LogInformation($"[ProcessMessageOnTimer] Printing App Settings via environment variables:");
            _logger.LogInformation($"[ProcessMessageOnTimer] AppSetting: ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

            _logger.LogInformation($"[ProcessMessageOnTimer] Printing App Settings via ConfigurationManager:");
            _logger.LogInformation($"[ProcessMessageOnTimer] AppSetting: CosmosDb:Containers:InspectionData = {_configuration.GetValue<string>("CosmosDb:Containers:InspectionData")}");
            _logger.LogInformation($"[ProcessMessageOnTimer] AppSetting: ASPNETCORE_ENVIRONMENT = {_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT")}");

        }

        private async Task SaveInspectionsToStorageTable(List<InspectionRecordAggregated> inspectionRecordAggregatedList)
        {
            // Iterate over each inspection:
            //  If we haven't seen this inspection before, write the inspection serial number to Azure Storage table
            if (inspectionRecordAggregatedList != null)
            {
                foreach (InspectionRecordAggregated inspectionRecordAggregated in inspectionRecordAggregatedList)
                {
                    _logger.LogInformation(
                        "[SaveInspectionsToStorageTable]: " +
                        $"Name: {inspectionRecordAggregated.Name} " +
                        $"City: {inspectionRecordAggregated.City} " +
                        $"Inspection_Result: {inspectionRecordAggregated.InspectionResult}");

                    // Determine if we already have data for the current inspection for the establishment
                    List<ExistingInspectionModel> existingInspectionModelsList = await _existingInspectionsTableProvider.QueryInspectionRecord(
                        inspectionRecordAggregated.InspectionSerialNum,
                        inspectionRecordAggregated.InspectionSerialNum);

                    if (existingInspectionModelsList.Count == 0)
                    {
                        // Write the inspection serial number and ID to Azure Storage table
                        await _existingInspectionsTableProvider.AddInspectionRecord(
                        inspectionRecordAggregated.InspectionSerialNum,
                        inspectionRecordAggregated.InspectionSerialNum);
                        _logger.LogInformation("[SaveInspectionsToStorageTable]: Added inspection record to Azure Storage Table.");
                    }
                    // Else, we've already seen this data so no reason to upsert it to Azure Storage table
                    else
                    {
                        _logger.LogInformation("[SaveInspectionsToStorageTable]: Inspection record already exists in Azure Storage Table.");
                    }
                }
            }
        }

        private async Task SaveInspectionsToCosmosDb(List<InspectionRecordAggregated> inspectionRecordAggregatedList)
        {
            // Iterate over each inspection:
            //  If we haven't seen this inspection before, write the complete data to Cosmos DB
            if (inspectionRecordAggregatedList != null)
            {
                foreach (InspectionRecordAggregated inspectionRecordAggregated in inspectionRecordAggregatedList)
                {
                    CosmosDbWriteDocument cosmosDbWriteDocument = new CosmosDbWriteDocument(inspectionRecordAggregated);
                    cosmosDbWriteDocument.id = cosmosDbWriteDocument.InspectionSerialNum;

                    _logger.LogInformation(
                        "[SaveInspectionsToCosmosDb]: " +
                        $"Name: {inspectionRecordAggregated.Name} " +
                        $"City: {inspectionRecordAggregated.City} " +
                        $"Inspection_Result: {inspectionRecordAggregated.InspectionResult}");

                    // Write the complete data to Cosmos DB
                    await _cosmosDbProvider.WriteDocument(cosmosDbWriteDocument);
                    _logger.LogInformation("[SaveInspectionsToCosmosDb]: Wrote inspection data to Cosmos DB.");
                }
            }
        }
    }
}
