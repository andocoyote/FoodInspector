using FoodInspectorModels;
using InspectionsReporter.Providers.EmailFormatProvider;
using InspectionsReporter.Providers.EmailMessageProvider;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// https://learn.microsoft.com/en-us/azure/azure-functions/functions-event-grid-blob-trigger?pivots=programming-language-csharp

namespace InspectionsReporter
{
    public class InspectionsReporter
    {
        private readonly IEmailMessageProvider _emailMessageProvider;
        private readonly ILogger<InspectionsReporter> _logger;

        public InspectionsReporter(
            IEmailMessageProvider emailMessageProvider,
            ILogger<InspectionsReporter> logger)
        {
            _emailMessageProvider = emailMessageProvider;
            _logger = logger;
        }

        [Function(nameof(InspectionsReporter))]
        public async Task Run([BlobTrigger("latest-inspections/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"[InspectionsReporter] C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");

            EstablishmentRecommendations? recommendations = JsonSerializer.Deserialize<EstablishmentRecommendations>(content);

            if (recommendations == null ||
                recommendations.Recommended == null ||
                recommendations.Unrecommended == null)
            {
                _logger.LogError("[InspectionsReporter] Failed to retrieve recommendations from Blob Storage trigger.");
            }
            else
            {
                string? htmlTable = EmailFormatProvider.GenerateHtmlTable(recommendations);

                if (string.IsNullOrEmpty(htmlTable))
                {
                    _logger.LogError("[InspectionsReporter] HTML table for Email message body is null or empty");
                    return;
                }

                _logger.LogInformation($"[InspectionsReporter] Sending email containing recommendations.");
                await _emailMessageProvider.SendEmailAsync(htmlTable);

                _logger.LogInformation($"[InspectionsReporter] Email containing recommendations sent.");
            }
        }
    }
}
