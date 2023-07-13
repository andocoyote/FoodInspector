using CommonFunctionality.CosmosDbProvider;
using CommonFunctionality.Model;
using FoodInspector.ExistingInspectionsTableProvider;
using FoodInspector.InspectionDataGatherer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodInspector
{
    public class Functions
    {
        // https://stackoverflow.com/questions/54155903/azure-webjob-read-appsettings-json-and-inject-configuration-to-timertrigger
        private readonly IConfiguration _configuration;
        private readonly IOptions<CosmosDbOptions> _cosmosDbOptions;
        private readonly IInspectionDataGatherer _inspectionDataGatherer;
        private readonly ICosmosDbProvider<InspectionData> _cosmosDbProvider;
        private readonly IExistingInspectionsTableProvider _existingInspectionsTableProvider;

        public Functions(
            IConfiguration configuration,
            IOptions<CosmosDbOptions> cosmosDbOptions,
            IInspectionDataGatherer inspectionDataGatherer,
            ICosmosDbProviderFactory<InspectionData> cosmosDbProviderFactory,
            IExistingInspectionsTableProvider existingInspectionsTableProvider)
        {
            _configuration = configuration;
            _cosmosDbOptions = cosmosDbOptions;
            _inspectionDataGatherer = inspectionDataGatherer;
            _cosmosDbProvider = cosmosDbProviderFactory.CreateProvider();
            _existingInspectionsTableProvider = existingInspectionsTableProvider;
        }

        public async Task ProcessMessageOnTimer(
            [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            try
            {
                logger.LogInformation("[ProcessMessageOnTimer] TimerTrigger fired.");

                logger.LogInformation($"[ProcessMessageOnTimer] Printing _cosmosDbOptions:");
                logger.LogInformation($"[ProcessMessageOnTimer] _cosmosDbOptions.Value.AccountEndpoint:{_cosmosDbOptions.Value.AccountEndpoint}");
                logger.LogInformation($"[ProcessMessageOnTimer] _cosmosDbOptions.Value.Database:{_cosmosDbOptions.Value.Database}");
                logger.LogInformation($"[ProcessMessageOnTimer] _cosmosDbOptions.Value.Containers.InspectionData:{_cosmosDbOptions.Value.Containers.InspectionData}");

                logger.LogInformation($"[ProcessMessageOnTimer] Printing environment variable AppSettings:");
                // https://stackoverflow.com/questions/45144771/how-to-receive-data-from-app-settings-azure-webapp-to-my-webjob
                logger.LogInformation($"[ProcessMessageOnTimer] AppSetting: ASPNETCORE_ENVIRONMENT={Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

                logger.LogInformation($"[ProcessMessageOnTimer] Printing ConfigurationManager AppSettings:");
                logger.LogInformation($"[ProcessMessageOnTimer] AppSetting: CosmosDb:Containers:InspectionData={_configuration.GetValue<string>("CosmosDb:Containers:InspectionData")}");
                logger.LogInformation($"[ProcessMessageOnTimer] AppSetting: ASPNETCORE_ENVIRONMENT={_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENTta")}");

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
