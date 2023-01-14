using InspectionDataEventCreator.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace InspectionDataEventStreamCreator
{
    // This Azure Function creates the core event stream from the raw InspectionData added to Cosmos DB by the FoodInspector WebJob.
    // These core events are written to the event store in Cosmos DB.
    // The core event stream contains events for a new violation or no violation.
    // Subsequent Azure Functions are triggered off of the event store change feed to add additional events to Event Grid
    public static class InspectionDataEventCreatorFunction
    {
        [FunctionName("InspectionDataEventCreatorFunction")]
        public static void Run([CosmosDBTrigger(
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

            log.LogInformation("[InspectionDataEventCreatorFunction] Finished processing change feed for InspectionData container.");
        }
    }
}
