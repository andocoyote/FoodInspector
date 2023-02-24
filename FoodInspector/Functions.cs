using FoodInspector.CosmosDbProvider;
using FoodInspector.EstablishmentsTableProvider;
using FoodInspector.ExistingInspectionsTableProvider;
using FoodInspector.InspectionDataGatherer;
using FoodInspector.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FoodInspector
{
    public class Functions
    {

        private readonly IInspectionDataGatherer _inspectionDataGatherer;
        private readonly ICosmosDbProvider<InspectionData> _cosmosDbProvider;
        private readonly IExistingInspectionsTableProvider _existingInspectionsTableProvider;

        public Functions(
            IInspectionDataGatherer inspectionDataGatherer,
            InspectionDataCosmosDbProviderFactory cosmosDbProviderFactory,
            IExistingInspectionsTableProvider existingInspectionsTableProvider)
        {
            _inspectionDataGatherer = inspectionDataGatherer;
            _cosmosDbProvider = cosmosDbProviderFactory.CreateProvider();
            _existingInspectionsTableProvider = existingInspectionsTableProvider;
        }

        public async Task ProcessMessageOnTimer(
            [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            logger.LogInformation("[ProcessMessageOnTimer] TimerTrigger fired.");

            try
            {
                // Query the food inspections API for the latest data
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

                        // Determine if we already have data for the current inspection for the establishment
                        List<ExistingInspectionModel> existingInspectionModelsList = await _existingInspectionsTableProvider.QueryInspectionRecord(
                            inspectionData.Inspection_Serial_Num,
                            inspectionData.id);

                        // If we haven't seen this inspection before, write the complete data to Cosmos DB
                        // and note the inspection serial number and ID to Azure Storage table
                        if (existingInspectionModelsList.Count == 0)
                        {
                            await _existingInspectionsTableProvider.AddInspectionRecord(
                            inspectionData.Inspection_Serial_Num,
                            inspectionData.id);
                            logger.LogInformation("[ProcessMessageOnTimer]: Added inspection record to Azure Storage Table.");

                            await _cosmosDbProvider.WriteDocument(inspectionData);
                            logger.LogInformation("[ProcessMessageOnTimer]: Wrote inspection data to Cosmos DB.");
                        }
                        // Else, we've already seen this data so no reason to upsert it to Cosmos DB
                        else
                        {
                            logger.LogInformation("[ProcessMessageOnTimer]: Inspection record already exists in Azure Storage Table.");
                        }
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
