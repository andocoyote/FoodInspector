using FoodInspector.DependencyInjection;
using FoodInspector.InspectionDataWriter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace FoodInspectorTests
{
    [TestClass]
    public class FoodInspectorCoreTests
    {
        private IServiceProvider _services = null;
        private ILoggerFactory _loggerFactory = null;

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
            config.["AZURE_SQL_CONNECTIONSTRING"] = SQLGeneralStorageConnectionString;

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
                inspectionDataWriter.UpsertData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestSQLDatabaseProvider] An exception was caught: {ex}");
            }
        }
    }
}