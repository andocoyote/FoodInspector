using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using CommonFunctionality.EventGrid;
using InspectionDataEventCreator.Model;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
            // builder.Services.AddSingleton<...>();

            builder.Services.AddOptions<EventGridOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("EventGrid").Bind(settings);
                });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: false, reloadOnChange: false)
                //.AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();

            var configRoot = builder.ConfigurationBuilder.Build();

            // appsettings*.json contains the Key Vault name, so add the Key Vault to the configuration
            builder.ConfigurationBuilder.AddAzureKeyVault(
                new Uri($"https://{configRoot["KeyVault:VaultName"]}.vault.azure.net/"),
                new DefaultAzureCredential());

            var config = builder.ConfigurationBuilder.Build();
        }
    }

    // This Azure Function creates the core event stream from the raw InspectionData added to Cosmos DB by the FoodInspector WebJob.
    // These core events are written to the event store in Cosmos DB.
    // The core event stream contains events for a new violation or no violation.
    // Subsequent Azure Functions are triggered off of the event store change feed to add additional events to Event Grid
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
        
        [FunctionName("InspectionDataEventCreatorFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "FoodInspector",
            containerName: "InspectionData",
            StartFromBeginning = true,
            Connection = "AzureCosmosDbConnectionString",
            LeaseConnection = "AzureCosmosDbConnectionString",
            CreateLeaseContainerIfNotExists = true,
            LeaseContainerName = "leases")]IReadOnlyList<InspectionData> input,
            ILogger logger)
        {
            logger.LogInformation("[InspectionDataEventCreatorFunction] About to process change feed for InspectionData container.");

            try
            {
                DisplayConfiguration(logger);

                // Connect to the EventGridPublisherClient to send events to Event Grid
                string eventGridEndpoint = "https://inspectionresults.westus3-1.eventgrid.azure.net/api/events";
                string eventGridAccessKey = _eventGridOptions.Value.InspectionResultsKey;

                EventGridPublisherClient client = new EventGridPublisherClient(
                    new Uri(eventGridEndpoint),
                    new AzureKeyCredential(eventGridAccessKey));

                // Iterate over each document added to Cosmos DB and send an event to Event Grid
                if (input != null && input.Count > 0)
                {
                    logger.LogInformation($"[InspectionDataEventCreatorFunction] Number of documents modified: {input.Count}");

                    using (var enumerator = input.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            logger.LogInformation($"[InspectionDataEventCreatorFunction] Sending event for document ID: {enumerator.Current.Id}");

                            string jsonString = JsonSerializer.Serialize<InspectionData>(enumerator.Current);

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
                logger.LogError($"[InspectionDataEventCreatorFunction] An exception was caught. Exception: {ex}");
            }

            logger.LogInformation("[InspectionDataEventCreatorFunction] Finished processing change feed for InspectionData container.");
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
