using FoodInspector.DependencyInjection;
using FoodInspector.EstablishmentsProvider;
using FoodInspector.InspectionDataWriter;
using FoodInspector.Model;
using HttpClientTest.HttpHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace FoodInspectorTests
{
    [TestClass]
    public class FoodInspectorCoreTests
    {
        private IServiceProvider _services = null;

        [TestInitialize]
        public void TestInitialize()
        {
            string username = "";
            string password = "";
            string SQLGeneralStorageConnectionString = $"Server=tcp:sql-general-storage.database.windows.net,1433;Initial Catalog=sqldb-general-storage;Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            
            IServiceCollection serviceCollection = new ServiceCollection();

            ContainerBuilder.ConfigureServices(serviceCollection);

            _services = serviceCollection.BuildServiceProvider();
            IConfiguration config = _services.GetRequiredService<IConfiguration>();
            config["AZURE_SQL_CONNECTIONSTRING"] = SQLGeneralStorageConnectionString;

            var configFile = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;


            configFile.Save(ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        [TestMethod]
        public void TestSQLDatabaseProvider()
        {
            try
            {
                IInspectionDataWriter inspectionDataWriter = _services.GetRequiredService<IInspectionDataWriter>();
                inspectionDataWriter.WriteData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestSQLDatabaseProvider] An exception was caught: {ex}");
            }
        }

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
    }
}