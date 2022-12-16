using FoodInspector.InspectionDataWriter;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FoodInspector
{
    public class Functions
    {

        private readonly IInspectionDataWriter _inspectionDataWriter;

        public Functions(IInspectionDataWriter inspectionDataWriter)
        {
            _inspectionDataWriter = inspectionDataWriter;
        }

        public async Task ProcessMessageOnTimer(
            [TimerTrigger("0 */15 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            logger.LogInformation("[ProcessMessageOnTimer] TimerTrigger fired.");

            try
            {
                await _inspectionDataWriter.UpsertData();
            }
            catch (Exception ex)
            {
                logger.LogInformation($"[ProcessMessageOnTimer] An exception was caught. Exception: {ex}");
            }

            logger.LogInformation("[ProcessMessageOnTimer] Processing completed.");
        }
    }
}
