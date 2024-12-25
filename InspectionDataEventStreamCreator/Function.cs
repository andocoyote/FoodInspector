using Azure.Messaging.EventGrid;
using CommonFunctionality.CosmosDbProvider;
using CommonFunctionality.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace InspectionDataEventStreamCreator
{
    public class InspectionDataEventCreatorFunction
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<EventGridOptions> _eventGridOptions;
        private readonly ILogger _logger;

        public InspectionDataEventCreatorFunction(
            IConfiguration configuration,
            IOptions<EventGridOptions> eventGridOptions,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _eventGridOptions = eventGridOptions;
            _logger = loggerFactory.CreateLogger<InspectionDataEventCreatorFunction>();
        }

        [Function("InspectionDataEventCreatorFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "FoodInspector",
            containerName: "InspectionRecordsAggregated",
            Connection = "AzureCosmosDbConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<CosmosDbReadDocument> input)
        {
            _logger.LogInformation("[InspectionDataEventCreatorFunction] About to process change feed for InspectionRecordsAggregated container.");

            try
            {
                DisplayConfiguration(_logger);

                // Connect to the EventGridPublisherClient to send events to Event Grid
                string eventGridEndpoint = "https://inspectionresults.westus3-1.eventgrid.azure.net/api/events";
                string eventGridAccessKey = _eventGridOptions.Value.InspectionResultsKey;

                EventGridPublisherClient client = new EventGridPublisherClient(
                    new Uri(eventGridEndpoint),
                    new Azure.AzureKeyCredential(eventGridAccessKey));

                // Iterate over each document added to Cosmos DB and send an event to Event Grid
                if (input != null && input.Count > 0)
                {
                    _logger.LogInformation($"[InspectionDataEventCreatorFunction] Number of documents modified: {input.Count}");

                    using (var enumerator = input.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            _logger.LogInformation($"[InspectionDataEventCreatorFunction] Sending event for document ID: {enumerator.Current.id}");

                            string jsonString = JsonSerializer.Serialize(enumerator.Current);

                            // Create the event to send to Event Grid
                            EventGridEvent eventGridEvent = new EventGridEvent(
                                "NewInspection",
                                "InspectionData.NewInspection",
                                "1.0",
                                jsonString);

                            // Send the event
                            await client.SendEventAsync(eventGridEvent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[InspectionDataEventCreatorFunction] An exception was caught. Exception: {ex}");
            }

            _logger.LogInformation("[InspectionDataEventCreatorFunction] Finished processing change feed for InspectionRecordsAggregated container.");
        }

        private void DisplayConfiguration(ILogger logger)
        {
            logger.LogInformation($"[InspectionDataEventCreatorFunction] Printing App Settings via IOptions classes:");
            logger.LogInformation($"[InspectionDataEventCreatorFunction] _eventGridOptions.Value.InspectionResultsKey={_eventGridOptions.Value.InspectionResultsKey}");

            logger.LogInformation($"[InspectionDataEventCreatorFunction] Printing App Settings via ConfigurationManager:");
            logger.LogInformation($"[InspectionDataEventCreatorFunction] AppSetting: KeyVault:VaultName = {_configuration.GetValue<string>("KeyVault:VaultName")}");
        }
    }
}
