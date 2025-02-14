using Azure.Messaging.ServiceBus;
using CommonFunctionality.CosmosDbProvider;
using FoodInspector.Providers.ExistingInspectionsTableProvider;
using FoodInspector.Providers.InspectionDataGatherer;
using FoodInspectorModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoodInspector
{
    public class Functions
    {
        // https://stackoverflow.com/questions/54155903/azure-webjob-read-appsettings-json-and-inject-configuration-to-timertrigger
        private readonly IConfiguration _configuration;
        private readonly IInspectionDataGatherer _inspectionDataGatherer;
        private readonly ICosmosDbProvider<CosmosDbWriteDocument, CosmosDbReadDocument> _cosmosDbProvider;
        private readonly IExistingInspectionsTableProvider _existingInspectionsTableProvider;
        private readonly ServiceBusSender _serviceBusSender;
        private ILogger _logger;

        public Functions(
            IConfiguration configuration,
            IInspectionDataGatherer inspectionDataGatherer,
            ICosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument> cosmosDbProviderFactory,
            IExistingInspectionsTableProvider existingInspectionsTableProvider,
            ServiceBusSender serviceBusSender)
        {
            _configuration = configuration;
            _inspectionDataGatherer = inspectionDataGatherer;
            _cosmosDbProvider = cosmosDbProviderFactory.CreateProvider();
            _existingInspectionsTableProvider = existingInspectionsTableProvider;
            _serviceBusSender = serviceBusSender;
        }

        public async Task LatestInspectionsGatherer(
            [TimerTrigger("0 0 22 * * Fri", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            _logger = logger;
            _logger.LogInformation("[LatestInspectionsGatherer] TimerTrigger fired.");

            try
            {
                // Query the food inspections API for the latest data
                List<InspectionRecordAggregated> inspectionRecordAggregatedList = await _inspectionDataGatherer.QueryAllInspections();

                _logger.LogInformation($"[LatestInspectionsGatherer] inspectionRecordAggregatedList count: {(inspectionRecordAggregatedList?.Count ?? -1)}.");

                await SaveInspectionsToStorageTableAsync(inspectionRecordAggregatedList);

                _logger.LogInformation($"[LatestInspectionsGatherer] inspectionRecordAggregatedList saved to Azure Storage.");

                await SaveInspectionsToCosmosDbAsync(inspectionRecordAggregatedList);

                _logger.LogInformation($"[LatestInspectionsGatherer] inspectionRecordAggregatedList saved to Azure Cosmos DB.");

                await SendServicBusMessageAsync(inspectionRecordAggregatedList);

                _logger.LogInformation($"[LatestInspectionsGatherer] message sent to Service Bus Queue.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[LatestInspectionsGatherer] An exception was caught. Exception: {ex}");
            }

            _logger.LogInformation("[LatestInspectionsGatherer] Processing completed.");
        }

        private async Task SaveInspectionsToStorageTableAsync(List<InspectionRecordAggregated> inspectionRecordAggregatedList)
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

        private async Task SaveInspectionsToCosmosDbAsync(List<InspectionRecordAggregated> inspectionRecordAggregatedList)
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
                    await _cosmosDbProvider.WriteDocumentAsync(cosmosDbWriteDocument);
                    _logger.LogInformation("[SaveInspectionsToCosmosDb]: Wrote inspection data to Cosmos DB.");
                }
            }
        }

        private async Task SendServicBusMessageAsync(List<InspectionRecordAggregated> inspectionRecordAggregatedList)
        {
            string message = JsonSerializer.Serialize(inspectionRecordAggregatedList);

            var serviceBusMessage = new ServiceBusMessage(message);
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
