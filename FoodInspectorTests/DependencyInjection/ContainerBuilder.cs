using CommonFunctionality.CosmosDbProvider;
using CommonFunctionality.Model;
using EventStore.CosmosDb;
using EventStore.Domain;
using FoodInspector.Providers.EstablishmentsProvider;
using FoodInspector.Providers.EstablishmentsTableProvider;
using FoodInspector.Providers.ExistingInspectionsTableProvider;
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
            serviceCollection.AddSingleton<IEstablishmentsTableProvider, Providers.EstablishmentsTableProvider.ExistingInspectionsTableProvider>();
            serviceCollection.AddSingleton<IEstablishmentsProvider, EstablishmentsProvider>();
            serviceCollection.AddSingleton<ICosmosDbProviderFactory<InspectionData>, InspectionDataCosmosDbProviderFactory>();
            serviceCollection.AddSingleton<IExistingInspectionsTableProvider, Providers.ExistingInspectionsTableProvider.ExistingInspectionsTableProvider>();
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
