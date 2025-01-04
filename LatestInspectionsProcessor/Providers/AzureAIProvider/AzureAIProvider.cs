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
        private string _jsonSchemaFileName { get; set; }

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
            _jsonSchemaFileName = _azureAIOptions.Value.JsonSchemaFileName;
        }

        public async Task<string> ProcessInspectionResults(List<InspectionRecordOpenAIRequestModel> inspectionRecordOpenAIRequestModels)
        {
            // Get the system message from Blob Storage
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(_systemMessageFileName);

            using MemoryStream systemMessageStream = new MemoryStream();
            await blobClient.DownloadToAsync(systemMessageStream);
            string systemMessage = Encoding.UTF8.GetString(systemMessageStream.ToArray());

            _logger.LogInformation("[ProcessInspectionResults] System message:");
            _logger.LogInformation(systemMessage);

            // Get the JSON schema for results from Blob Storage
            // Download the blob content to a stream
            blobClient = containerClient.GetBlobClient(_jsonSchemaFileName);
            string jsonSchema = string.Empty;

            using MemoryStream jsonSchemaStream = new MemoryStream();
            await blobClient.DownloadToAsync(jsonSchemaStream);

            // Convert the stream to a string
            jsonSchemaStream.Position = 0;
            using StreamReader reader = new StreamReader(jsonSchemaStream);
            jsonSchema = await reader.ReadToEndAsync();

            _logger.LogInformation("[ProcessInspectionResults] JSON schema:");
            _logger.LogInformation(jsonSchema);

            // Configure the OpenAI LLM
            var OpenAIClient = new AzureOpenAIClient(new Uri(_azureAIOptions.Value.Endpoint), new DefaultAzureCredential());
            var OpenAIChatClient = OpenAIClient.GetChatClient(_azureAIOptions.Value.Deployment);

            string inpections = JsonSerializer.Serialize(inspectionRecordOpenAIRequestModels);

            // Use options to make the LLM apply the JSON schema to its output
            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "food_inspections_processing",
                    jsonSchema: BinaryData.FromString(jsonSchema),
                    jsonSchemaIsStrict: true)
            };

            // Query the OpenAI LLM
            ChatCompletion completion = OpenAIChatClient.CompleteChat(
                [
                    new SystemChatMessage(systemMessage),
                    new UserChatMessage(inpections)
                ],
                options);

            string chatResults = string.Empty;
            if (completion == null || completion.Content.Count == 0)
            {
                _logger.LogError("[ProcessInspectionResults] Call to CompleteChat failed to return results.");
            }
            else
            {
                _logger.LogInformation($"[ProcessInspectionResults] Call to CompleteChat returned {completion.Content.Count} result(s).");

                chatResults = completion.Content[0].Text;
            }

            return chatResults;
        }
    }
}
