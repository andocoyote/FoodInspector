using InspectionDataEventCreator.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace InspectionDataEventStreamCreator
{
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
