using FoodInspector.CosmosDbProvider;
using FoodInspector.InspectionDataGatherer;
using FoodInspector.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FoodInspector
{
    public class Functions
    {

        private readonly IInspectionDataGatherer _inspectionDataGatherer;
        private readonly ICosmosDbProvider _cosmosDbProvider;

        public Functions(IInspectionDataGatherer inspectionDataGatherer, ICosmosDbProvider cosmosDbProvider)
        {
            _inspectionDataGatherer = inspectionDataGatherer;
            _cosmosDbProvider= cosmosDbProvider;
        }

        public async Task ProcessMessageOnTimer(
            [TimerTrigger("0 */15 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            logger.LogInformation("[ProcessMessageOnTimer] TimerTrigger fired.");

            try
            {
                List<InspectionData> inspectionDataList = await _inspectionDataGatherer.GatherData();

                logger.LogInformation($"[ProcessMessageOnTimer] inspectionDataList count: {(inspectionDataList?.Count ?? -1)}.");

                if (inspectionDataList != null)
                {
                    foreach (InspectionData inspectionData in inspectionDataList)
                    {
                        logger.LogInformation(
                            "[ProcessMessageOnTimer]: " +
                            $"Name: {inspectionData.Name} " +
                            $"City: {inspectionData.City} " +
                            $"City: {inspectionData.Inspection_Result}");

                        await _cosmosDbProvider.WriteDocument(inspectionData);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"[ProcessMessageOnTimer] An exception was caught. Exception: {ex}");
            }

            logger.LogInformation("[ProcessMessageOnTimer] Processing completed.");
        }
    }
}
