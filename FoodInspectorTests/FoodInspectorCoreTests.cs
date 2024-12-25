using CommonFunctionality.CosmosDbProvider;
using FoodInspector.DependencyInjection;
using FoodInspector.Providers.EstablishmentsProvider;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodInspectorTests
{
    [TestClass]
    public class FoodInspectorCoreTests
    {
        private IServiceProvider _services = null;
        private IConfiguration _configuration;

        [TestInitialize]
        public void TestInitialize()
        {
            string username = "";
            string password = "";
            string SQLGeneralStorageConnectionString = $"Server=tcp:sql-general-storage.database.windows.net,1433;Initial Catalog=sqldb-general-storage;Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            IServiceCollection serviceCollection = new ServiceCollection();
            ContainerBuilder.ConfigureServices(serviceCollection, _configuration);

            // Add the required connection strings and settings to the global ConfigurationManager
            _services = serviceCollection.BuildServiceProvider();
            _configuration = _services.GetRequiredService<IConfiguration>();
            _configuration["AZURE_SQL_CONNECTIONSTRING"] = SQLGeneralStorageConnectionString;

            //serviceCollection.AddDbContext<>();

            Microsoft.Extensions.Configuration.ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        }

        [TestMethod]
        public void DisplayConfiguration()
        {
            string connectionstring = _configuration["AZURE_SQL_CONNECTIONSTRING"];
            Console.WriteLine($"{connectionstring}");
        }

        /*
        [TestMethod]
        public async Task CallFoodEstablishmentInspectionDataAPI()
        {
            try
            {
                ICommonServiceLayerProvider commonServiceLayerProvider = _services.GetRequiredService<ICommonServiceLayerProvider>();

                List<InspectionData> inspectionData = await commonServiceLayerProvider.GetInspections("", "Bothell", "2022-01-01");

                Console.WriteLine($"{inspectionData.Count} records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CallFoodEstablishmentInspectionDataAPI] An exception was caught: {ex}");
            }
        }
        */

        [TestMethod]
        public void ReadEstablishmentsFile()
        {
            try
            {
                IEstablishmentsProvider establishmentsProvider = _services.GetRequiredService<IEstablishmentsProvider>();

                List<EstablishmentsModel> establishments = establishmentsProvider.ReadEstablishmentsFile();

                foreach (EstablishmentsModel establishment in establishments)
                {
                    Console.WriteLine(
                        "[ReadEstablishmentsFile]: " +
                        $"PartitionKey: {establishment.PartitionKey} " +
                        $"RowKey: {establishment.RowKey} " +
                        $"Name: {establishment.Name} " +
                        $"City: {establishment.City}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadEstablishmentsFile] An exception was caught: {ex}");
            }
        }

        /*
        [TestMethod]
        public async Task GetFoodEstablishmentInspectionResults()
        {
            try
            {
                IEstablishmentsProvider establishmentsProvider = _services.GetRequiredService<IEstablishmentsProvider>();
                ICommonServiceLayerProvider commonServiceLayerProvider = _services.GetRequiredService<ICommonServiceLayerProvider>();

                List<EstablishmentsModel> establishmentsList = establishmentsProvider.ReadEstablishmentsFile();
                List<InspectionData> inspectionDataList = await commonServiceLayerProvider.GetInspections(establishmentsList);

                Console.WriteLine($"[GetFoodEstablishmentInspectionResults] inspectionDataList count: {(inspectionDataList?.Count ?? -1)}.");

                if (inspectionDataList != null)
                {
                    foreach (InspectionData inspectionData in inspectionDataList)
                    {
                        Console.WriteLine(
                            "[GetFoodEstablishmentInspectionResults]: " +
                            $"\nName: {inspectionData.Name} " +
                            $"\n\tCity: {inspectionData.City} " +
                            $"\n\tInspection Date: {inspectionData.Inspection_Date} " +
                            $"\n\tResult: {inspectionData.Inspection_Result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetFoodEstablishmentInspectionResults] An exception was caught: {ex}");
            }
        }
        */

        /*
        [TestMethod]
        public async Task WriteInspectionDataToCosmosDB()
        {
            try
            {
                IEstablishmentsProvider establishmentsProvider = _services.GetRequiredService<IEstablishmentsProvider>();
                ICommonServiceLayerProvider commonServiceLayerProvider = _services.GetRequiredService<ICommonServiceLayerProvider>();

                ICosmosDbProviderFactory<InspectionData> factory = _services.GetRequiredService<ICosmosDbProviderFactory<InspectionData>>();
                ICosmosDbProvider<InspectionData> cosmosDbProvider = factory.CreateProvider();

                List<EstablishmentsModel> establishmentsList = establishmentsProvider.ReadEstablishmentsFile();
                List<InspectionData> inspectionDataList = await commonServiceLayerProvider.GetInspections(establishmentsList);

                Console.WriteLine($"[WriteInspectionDataToCosmosDB] inspectionDataList count: {(inspectionDataList?.Count ?? -1)}.");

                if (inspectionDataList != null)
                {
                    foreach (InspectionData inspectionData in inspectionDataList)
                    {
                        Console.WriteLine(
                            "[WriteInspectionDataToCosmosDB]: " +
                            $"Name: {inspectionData.Name} " +
                            $"City: {inspectionData.City} " +
                            $"Inspection_Result: {inspectionData.Inspection_Result}");

                        if (inspectionData != null)
                        {
                            await cosmosDbProvider.WriteDocument(inspectionData);
                        }
                        else
                        {
                            Console.WriteLine("[WriteInspectionDataToCosmosDB]: InspectionData is null.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WriteInspectionDataToCosmosDB] An exception was caught. Exception: {ex}");
            }
        }
        */

        [TestMethod]
        public async Task ReadInspectionDataFromCosmosDB()
        {
            try
            {
                ICosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument> factory = _services.GetRequiredService<ICosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument>>();
                ICosmosDbProvider<CosmosDbWriteDocument, CosmosDbReadDocument> cosmosDbProvider = factory.CreateProvider();

                CosmosDbReadDocument inspectionData = await cosmosDbProvider.ReadDocument("0", new PartitionKey("DAJWHFI6N"));

                if (inspectionData != null)
                {
                    Console.WriteLine(
                            "[ReadInspectionDataFromCosmosDB]: " +
                            $"Name: {inspectionData.Name} " +
                            $"City: {inspectionData.City} " +
                            $"Inspection_Result: {inspectionData.InspectionResult}");
                }
                else
                {
                    Console.WriteLine("[ReadInspectionDataFromCosmosDB]: InspectionData is null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadInspectionDataFromCosmosDB] An exception was caught. Exception: {ex}");
            }
        }
    }
}