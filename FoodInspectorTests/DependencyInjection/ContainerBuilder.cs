using FoodInspector.CosmosDbProvider;
using FoodInspector.EstablishmentsProvider;
using FoodInspector.InspectionDataWriter;
using FoodInspector.KeyVaultProvider;
using FoodInspector.SQLDatabaseProvider;
using FoodInspector.EstablishmentsTableProvider;
using HttpClientTest.HttpHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FoodInspector.ExistingInspectionsTableProvider;

namespace FoodInspector.DependencyInjection
{
    public static class ContainerBuilder
    {
        public static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration, Microsoft.Extensions.Configuration.ConfigurationManager>();
            serviceCollection.AddSingleton<IKeyVaultProvider, KeyVaultProvider.KeyVaultProvider>();
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddSingleton<IInspectionDataWriter, InspectionDataWriter.InspectionDataWriter>();
            serviceCollection.AddSingleton<ICommonServiceLayerProvider, CommonServiceLayerProvider>();
            serviceCollection.AddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider.SQLDatabaseProvider>();
            serviceCollection.AddSingleton<IEstablishmentsTableProvider, EstablishmentsTableProvider.ExistingInspectionsTableProvider>();
            serviceCollection.AddSingleton<IEstablishmentsProvider, EstablishmentsProvider.EstablishmentsProvider>();
            serviceCollection.AddSingleton<ICosmosDbProvider, CosmosDbProvider.CosmosDbProvider>();
            serviceCollection.AddSingleton<IExistingInspectionsTableProvider, ExistingInspectionsTableProvider.ExistingInspectionsTableProvider>();
            serviceCollection.AddLogging();
        }
    }
}
