using Azure.Storage.Blobs;
using FoodInspectorModels;
using LatestInspectionsProcessor.Models;
using LatestInspectionsProcessor.Providers.AzureAIProvider;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace LatestInspectionsProcessor
{
    public class Function
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IAzureAIProvider _azureAIProvider;
        private readonly ILogger<Function> _logger;

        private const string BLOB_NAME_PREFIX = "LatestInspections_";
        private string containerName = "latest-inspections";

        public Function(
            BlobServiceClient blobServiceClient,
            IAzureAIProvider azureAIProvider,
            ILogger<Function> logger)
        {
            _blobServiceClient = blobServiceClient;
            _azureAIProvider = azureAIProvider;
            _logger = logger;
        }

        // Connection = "ServiceBusConnection" tells Azure that ServiceBusConnection__fullyQualifiedNamespace app setting
        // contains the Service Bus Namespace
        [Function(nameof(Function))]
        public async Task Run(
            [ServiceBusTrigger("inspectionrecordsaggregatedqueue", Connection = "ServiceBusConnection")]
            string inspectionsQueueItem)
        {
            _logger.LogInformation($"[LatestInspectionsProcessor] Message Body: {inspectionsQueueItem}");

            try
            {
                List<InspectionRecordAggregated>? inspectionRecordAggregatedList = 
                    JsonSerializer.Deserialize<List<InspectionRecordAggregated>>(inspectionsQueueItem);

                if (inspectionRecordAggregatedList == null)
                {
                    _logger.LogError("[LatestInspectionsProcessor] Inspection data from Service Bus is null.");
                    return;
                }

                // Only send the minimum info required for ChatGPT to make recommendations
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

                _logger.LogInformation(@$"[LatestInspectionsProcessor] Converted {inspectionRecordOpenAIRequestModels.Count} " +
                    "CosmosDbReadDocument documents to InspectionRecordOpenAIRequestModel items.");

                string chatResultJSON = await _azureAIProvider.ProcessInspectionResults(inspectionRecordOpenAIRequestModels);

                _logger.LogInformation($"[LatestInspectionsProcessor] AI recommendations retrieved: {(string.IsNullOrEmpty(chatResultJSON) ? "false" : "true")}");

                // Now that we have the recommendations, use them to create the larger data model with all establishment properties
                RecommendationsModel? recommendationsModel = JsonSerializer.Deserialize<RecommendationsModel>(chatResultJSON);

                EstablishmentRecommendations establishmentRecommendations = new();
                establishmentRecommendations.Recommended = new List<InspectionRecordAggregated>();
                establishmentRecommendations.Unrecommended = new List<InspectionRecordAggregated>();

                foreach (string establishment in recommendationsModel?.Recommended ?? Enumerable.Empty<string>())
                {
                    InspectionRecordAggregated? record = inspectionRecordAggregatedList.Where(doc => doc.ProgramIdentifier == establishment).FirstOrDefault();

                    if (record != null)
                    {
                        establishmentRecommendations.Recommended.Add(record);
                    }
                }

                foreach (string establishment in recommendationsModel?.Unrecommended ?? Enumerable.Empty<string>())
                {
                    InspectionRecordAggregated? record = inspectionRecordAggregatedList.Where(doc => doc.ProgramIdentifier == establishment).FirstOrDefault();

                    if (record != null)
                    {
                        establishmentRecommendations.Unrecommended.Add(record);
                    }
                }

                string serializedRecs = JsonSerializer.Serialize(establishmentRecommendations);

                // Upload recommendations to Blob Storage
                if (!string.IsNullOrEmpty(serializedRecs))
                {
                    await UploadRecommendationsBlobAsync(serializedRecs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[LatestInspectionsProcessor] Exception caught while processing latest inspections: {ex}");
            }
        }

        private async Task UploadRecommendationsBlobAsync(string chatResultJSON)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Convert JSON string to a stream and upload it to Blob Storage
            using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(chatResultJSON));

            string blobName = BLOB_NAME_PREFIX + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".json";

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, overwrite: true);

            Console.WriteLine($"Blob '{blobName}' uploaded to container '{containerName}'.");
        }
    }
}
