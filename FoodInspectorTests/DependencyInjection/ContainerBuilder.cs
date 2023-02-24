﻿using CommonFunctionality.KeyVaultProvider;
using FoodInspector.CosmosDbProvider;
using FoodInspector.EstablishmentsProvider;
using FoodInspector.EstablishmentsTableProvider;
using FoodInspector.ExistingInspectionsTableProvider;
using FoodInspector.InspectionDataWriter;
using FoodInspector.Model;
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
            serviceCollection.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration, Microsoft.Extensions.Configuration.ConfigurationManager>();
            serviceCollection.AddSingleton<IKeyVaultProvider, CommonFunctionality.KeyVaultProvider.KeyVaultProvider>();
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddSingleton<IInspectionDataWriter, InspectionDataWriter.InspectionDataWriter>();
            serviceCollection.AddSingleton<ICommonServiceLayerProvider, CommonServiceLayerProvider>();
            serviceCollection.AddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider.SQLDatabaseProvider>();
            serviceCollection.AddSingleton<IEstablishmentsTableProvider, EstablishmentsTableProvider.ExistingInspectionsTableProvider>();
            serviceCollection.AddSingleton<IEstablishmentsProvider, EstablishmentsProvider.EstablishmentsProvider>();
            //serviceCollection.AddSingleton<ICosmosDbProvider<FoodInspector.Model.InspectionData>, InspectionDataCosmosDbProvider>();
            serviceCollection.AddSingleton<ICosmosDbProviderFactory<InspectionData>, InspectionDataCosmosDbProviderFactory>();
            serviceCollection.AddSingleton<IExistingInspectionsTableProvider, ExistingInspectionsTableProvider.ExistingInspectionsTableProvider>();
            serviceCollection.AddLogging();
        }
    }
}
