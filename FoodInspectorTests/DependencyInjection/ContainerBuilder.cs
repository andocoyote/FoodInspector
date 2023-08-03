using CommonFunctionality.CosmosDbProvider;
using CommonFunctionality.EventGrid;
using CommonFunctionality.Model;
using EventStore.CosmosDb;
using EventStore.Domain;
using FoodInspector.EstablishmentsProvider;
using FoodInspector.EstablishmentsTableProvider;
using FoodInspector.ExistingInspectionsTableProvider;
using FoodInspector.InspectionDataWriter;
using FoodInspector.SQLDatabaseProvider;
using HttpClientTest.HttpHelpers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoodInspector.DependencyInjection
{
    public static class ContainerBuilder
    {
        public static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration config)
        {
            serviceCollection.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration, Microsoft.Extensions.Configuration.ConfigurationManager>();
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddSingleton<IInspectionDataWriter, InspectionDataWriter.InspectionDataWriter>();
            serviceCollection.AddSingleton<ICommonServiceLayerProvider, CommonServiceLayerProvider>();
            serviceCollection.AddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider.SQLDatabaseProvider>();
            serviceCollection.AddSingleton<IEstablishmentsTableProvider, EstablishmentsTableProvider.ExistingInspectionsTableProvider>();
            serviceCollection.AddSingleton<IEstablishmentsProvider, EstablishmentsProvider.EstablishmentsProvider>();
            serviceCollection.AddSingleton<ICosmosDbProviderFactory<InspectionData>, InspectionDataCosmosDbProviderFactory>();
            serviceCollection.AddSingleton<IExistingInspectionsTableProvider, ExistingInspectionsTableProvider.ExistingInspectionsTableProvider>();
            serviceCollection.AddSingleton<ICosmosClient<TestEvent>, CosmosClient<TestEvent>>();
            serviceCollection.AddLogging();

            serviceCollection.AddOptions<EventStore.CosmosDb.CosmosClientOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    config.GetSection("EventStore").Bind(settings);
                });
        }
    }
}
