using FoodInspector.InspectionDataWriter;
using FoodInspector.KeyVaultProvider;
using FoodInspector.SQLDatabaseProvider;
using HttpClientTest.HttpHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoodInspector.DependencyInjection
{
    public static class ContainerBuilder
    {
        public static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration, Microsoft.Extensions.Configuration.ConfigurationManager > ();
            serviceCollection.AddSingleton<IKeyVaultProvider, FoodInspector.KeyVaultProvider.KeyVaultProvider>();
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddSingleton<IInspectionDataWriter, FoodInspector.InspectionDataWriter.InspectionDataWriter>();
            serviceCollection.AddSingleton<ICommonServiceLayerProvider, CommonServiceLayerProvider>();
            serviceCollection.AddSingleton<ISQLDatabaseProvider, FoodInspector.SQLDatabaseProvider.SQLDatabaseProvider>();
            serviceCollection.AddLogging();
        }
    }
}
