// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

// The following usings are for connecting directly to Event Grid
//using Azure.Messaging.EventGrid;
//using Microsoft.Azure.WebJobs.Extensions.EventGrid;

using InspectionEvaluator.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

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
        public static void Run([ServiceBusTrigger("newinspectionqueue")]string queueItem, ILogger log)
        {
            log.LogInformation("[InspectionEvaluator] About to process event from Event Grid via Service Bus Queue.");

            try
            {
                log.LogInformation($"[InspectionEvaluator] Raw message from Queue: {queueItem}");

                // Deserialize the message containing the event from the Service Bus Queue
                ServiceBusQueueEvent serviceBusQueueEvent = JsonSerializer.Deserialize<ServiceBusQueueEvent>(queueItem);

                // Deserialize the data portion of the event: this contains the inspection results
                InspectionData inspectionData = JsonSerializer.Deserialize<InspectionData>(serviceBusQueueEvent.data);

                log.LogInformation($"[InspectionEvaluator] Inspection for {inspectionData.Name}. Inspection type: {inspectionData.Inspection_Type}. Violation: {inspectionData.Violation_Description}.");
            }
            catch(Exception e)
            {
                log.LogError($"[InspectionEvaluator] Exception occurred processing message from Service Bus Queue.  Exception: {e}");
            }

            log.LogInformation("[InspectionEvaluator] Finished processing event from Event Grid via Service Bus Queue.");
        }
    }
}
