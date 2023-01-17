// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace InspectionEvaluator
{
    public static class InspectionEvaluator
    {
        /*[FunctionName("InspectionEvaluator")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation($"[InspectionEvaluator] Event: {eventGridEvent.Data.ToString()}");
        }*/

        [FunctionName("InspectionEvaluator")]
        public static void Run([ServiceBusTrigger("newinspectionqueue")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"[InspectionEvaluator] Message: {myQueueItem}");
        }
    }
}
