using Azure;
using Azure.Messaging.EventGrid;
using CommonFunctionality.KeyVaultProvider;
using InspectionDataEventCreator.Model;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Dependency Injection for Azure Functions:
//  https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
[assembly: FunctionsStartup(typeof(InspectionDataEventStreamCreator.Startup))]

namespace InspectionDataEventStreamCreator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IKeyVaultProvider, CommonFunctionality.KeyVaultProvider.KeyVaultProvider>();
        }
    }

    // This Azure Function creates the core event stream from the raw InspectionData added to Cosmos DB by the FoodInspector WebJob.
    // These core events are written to the event store in Cosmos DB.
    // The core event stream contains events for a new violation or no violation.
    // Subsequent Azure Functions are triggered off of the event store change feed to add additional events to Event Grid
    public class InspectionDataEventCreatorFunction
    {
        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ILogger _logger;

        public InspectionDataEventCreatorFunction(
            IKeyVaultProvider keyVaultProvider,
            ILoggerFactory loggerFactory)
        {
            _keyVaultProvider = keyVaultProvider;
            _logger = loggerFactory.CreateLogger<InspectionDataEventCreatorFunction>();
        }
        
        [FunctionName("InspectionDataEventCreatorFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "FoodInspector",
            containerName: "InspectionData",
            StartFromBeginning = true,
            Connection = "AzureCosmosDbConnectionString",
            LeaseConnection = "AzureCosmosDbConnectionString",
            CreateLeaseContainerIfNotExists = true,
            LeaseContainerName = "leases")]IReadOnlyList<InspectionData> input,
            ILogger log)
        {
            log.LogInformation("[InspectionDataEventCreatorFunction] About to process change feed for InspectionData container.");

            if (input != null && input.Count > 0)
            {
                log.LogInformation($"[InspectionDataEventCreatorFunction] Number of documents modified: {input.Count}");

                using (var enumerator = input.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        log.LogInformation($"[InspectionDataEventCreatorFunction] Document Id: {enumerator.Current.Id}");
                    }
                }
            }

            string eventGridEndpoint = "https://inspectionresults.westus3-1.eventgrid.azure.net/api/events";
            string eventGridAccessKey = await _keyVaultProvider.GetKeyVaultSecret(KeyVaultSecretNames.eventGridTopicInspectionResultsKey);

            log.LogInformation($"[InspectionDataEventCreatorFunction] EventGrid access key: {eventGridAccessKey}");

            EventGridPublisherClient client = new EventGridPublisherClient(
                new Uri(eventGridEndpoint),
                new AzureKeyCredential(eventGridAccessKey));

            EventGridEvent eventGridEvent = new EventGridEvent(
                "ExampleEventSubject",
                "Example.EventType",
                "1.0",
                "This is the event data");

            // Send the event
            await client.SendEventAsync(eventGridEvent);

            log.LogInformation("[InspectionDataEventCreatorFunction] Finished processing change feed for InspectionData container.");
        }
    }
}
