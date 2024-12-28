using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;
using FoodInspectorModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

namespace LatestInspectionsProcessor.Providers.AzureAIProvider
{
    public class AzureAIProvider : IAzureAIProvider
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger _logger;
        IOptions<AzureAIOptions> _azureAIOptions;

        private string _containerName { get; set; }
        private string _systemMessageFileName { get; set; }

        public AzureAIProvider(
            BlobServiceClient blobServiceClient,
            IOptions<AzureAIOptions> azureAIOptions,
            ILoggerFactory loggerFactory)
        {
            _blobServiceClient = blobServiceClient;
            _azureAIOptions = azureAIOptions;
            _logger = loggerFactory.CreateLogger<AzureAIProvider>();

            _containerName = _azureAIOptions.Value.SystemMessageBlobContainer;
            _systemMessageFileName = _azureAIOptions.Value.SystemMessageFileName;
        }

        public async Task<string> ProcessInspectionResults(List<InspectionRecordOpenAIRequestModel> inspectionRecordOpenAIRequestModels)
        {
            // Get the system message from Blob Storage
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(_systemMessageFileName);

            using MemoryStream memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            string systemMessage = Encoding.UTF8.GetString(memoryStream.ToArray());

            Console.WriteLine("[ProcessInspectionResults] System message:");
            Console.WriteLine(systemMessage);

            // Configure the OpenAI LLM
            var OpenAIClient = new AzureOpenAIClient(new Uri(_azureAIOptions.Value.Endpoint), new DefaultAzureCredential());
            var OpenAIChatClient = OpenAIClient.GetChatClient(_azureAIOptions.Value.Deployment);

            string inpections = JsonSerializer.Serialize(inspectionRecordOpenAIRequestModels);

            // Query the OpenAI LLM
            ChatCompletion completion = OpenAIChatClient.CompleteChat(
            [
                new SystemChatMessage(systemMessage),
                new UserChatMessage(inpections)
            ]);

            return completion.Content[0].Text;
        }
    }
}
