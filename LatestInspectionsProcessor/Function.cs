using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommonFunctionality.CosmosDbProvider;
using FoodInspectorModels;
using LatestInspectionsProcessor.Providers.AzureAIProvider;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LatestInspectionsProcessor
{
    public class Function
    {
        private readonly ICosmosDbProvider<CosmosDbWriteDocument, CosmosDbReadDocument> _cosmosDbProvider;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IAzureAIProvider _azureAIProvider;
        private readonly ILogger<Function> _logger;

        private const string BLOB_NAME_PREFIX = "LatestInspections_";
        private string containerName = "latest-inspections";

        public Function(
            ICosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument> cosmosDbProviderFactory,
            BlobServiceClient blobServiceClient,
            IAzureAIProvider azureAIProvider,
            ILogger<Function> logger)
        {
            _cosmosDbProvider = cosmosDbProviderFactory.CreateProvider();
            _blobServiceClient = blobServiceClient;
            _azureAIProvider = azureAIProvider;
            _logger = logger;
        }

        // Connection = "ServiceBusConnection" tells Azure that ServiceBusConnection__fullyQualifiedNamespace app setting
        // contains the Service Bus Namespace
        [Function(nameof(Function))]
        public async Task Run(
            [ServiceBusTrigger("inspectionrecordsaggregatedqueue", Connection = "ServiceBusConnection")]
            string myQueueItem)
        {
            _logger.LogInformation("[LatestInspectionsProcessor] Message Body: {body}", myQueueItem);

            // Get latest inspection documents from Cosmos DB for all establishments
            List<CosmosDbReadDocument> cosmosDbDocs = await _cosmosDbProvider.QueryLatestInspectionRecordsAsync();

            _logger.LogInformation($"[LatestInspectionsProcessor] Retrieved {cosmosDbDocs.Count} documents from Cosmos DB.");

            List<InspectionRecordOpenAIRequestModel> inspectionRecordOpenAIRequestModels =
                    cosmosDbDocs.Select(i => new InspectionRecordOpenAIRequestModel()
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

            // Upload recommendations to Blob Storage
            await UploadRecommendationsBlobAsync(chatResultJSON);
        }

        private async Task UploadRecommendationsBlobAsync(string chatResultJSON)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await foreach (BlobItem blob in containerClient.GetBlobsAsync())
            {
                _logger.LogInformation($"[LatestInspectionsProcessor] Found blob {blob.Name}.");
            }

            // Convert JSON string to a stream and upload it to Blob Storage
            using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(chatResultJSON));

            string blobName = BLOB_NAME_PREFIX + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".json";

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, overwrite: true);

            Console.WriteLine($"Blob '{blobName}' uploaded to container '{containerName}'.");
        }
    }
}
