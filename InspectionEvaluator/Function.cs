using Azure.Messaging.ServiceBus;
using InspectionEvaluator.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InspectionEvaluator
{
    public class Function
    {
        private readonly ILogger<Function> _logger;

        public Function(ILogger<Function> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function))]
        public async Task Run(
            [ServiceBusTrigger("newinspectionqueue", Connection = "AzureWebJobsServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            {
                _logger.LogInformation("[InspectionEvaluator] About to process event from Event Grid via Service Bus Queue.");

                try
                {
                    _logger.LogInformation($"[InspectionEvaluator] Raw message from Queue: {message.Body}");

                    // Deserialize the message containing the event from the Service Bus Queue
                    ServiceBusQueueEvent serviceBusQueueEvent = JsonSerializer.Deserialize<ServiceBusQueueEvent>(message.Body);

                    // Deserialize the data portion of the event: this contains the inspection results
                    InspectionData inspectionData = JsonSerializer.Deserialize<InspectionData>(serviceBusQueueEvent.data);

                    _logger.LogInformation($"[InspectionEvaluator] Inspection for {inspectionData.Name}. Inspection type: {inspectionData.Inspection_Type}. Violation: {inspectionData.Violation_Description}.");
                }
                catch (Exception e)
                {
                    _logger.LogError($"[InspectionEvaluator] Exception occurred processing message from Service Bus Queue.  Exception: {e}");
                }

                _logger.LogInformation("[InspectionEvaluator] Finished processing event from Event Grid via Service Bus Queue.");
            }
        }
    }
}
