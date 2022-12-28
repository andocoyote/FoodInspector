using DotNetCoreSqlDb.Models;
using FoodInspector.CosmosDbProvider;
using FoodInspector.EstablishmentsProvider;
using FoodInspector.InspectionDataGatherer;
using FoodInspector.InspectionDataWriter;
using FoodInspector.KeyVaultProvider;
using FoodInspector.SQLDatabaseProvider;
using FoodInspector.StorageTableProvider;
using HttpClientTest.HttpHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Creating WebJobs that target .NET
// https://learn.microsoft.com/en-us/azure/app-service/webjobs-sdk-get-started

// Creating loggers from LoggerFactory
// https://stackoverflow.com/questions/55049683/ilogger-injected-via-constructor-for-http-trigger-functions-with-azure-function

namespace FoodInspector
{
    internal class Program
    {
        static async Task Main()
        {
            Microsoft.Extensions.Configuration.ConfigurationManager configurationManager = new ConfigurationManager();
            var builder = new HostBuilder();

            // Configure the Dependency Injection container
            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IKeyVaultProvider, FoodInspector.KeyVaultProvider.KeyVaultProvider>();
                services.AddSingleton<ILoggerFactory, LoggerFactory>();
                services.AddSingleton<IInspectionDataWriter, FoodInspector.InspectionDataWriter.InspectionDataWriter>();
                services.AddSingleton<ICommonServiceLayerProvider, CommonServiceLayerProvider>();
                services.AddSingleton<ISQLDatabaseProvider, FoodInspector.SQLDatabaseProvider.SQLDatabaseProvider>();
                services.AddSingleton<IStorageTableProvider, StorageTableProvider.StorageTableProvider>();
                services.AddSingleton<IInspectionDataGatherer, InspectionDataGatherer.InspectionDataGatherer>();
                services.AddSingleton<IEstablishmentsProvider, EstablishmentsProvider.EstablishmentsProvider>();
                services.AddSingleton<ICosmosDbProvider, CosmosDbProvider.CosmosDbProvider>();
                services.AddDbContext<FoodInspectorDatabaseContext>(options =>
                    options.UseSqlServer(configurationManager.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));
                services.AddLogging();
            });

            // The AddConsole method adds console logging to the configuration
            builder.ConfigureLogging((context, b) =>
            {
                b.SetMinimumLevel(LogLevel.Information);
                b.AddConsole();
                b.AddApplicationInsightsWebJobs(o => { o.ConnectionString = configurationManager.GetConnectionString("AzureWebJobsDashboard"); });
            });

            // The ConfigureWebJobs extension method initializes the WebJobs host
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddTimers();
            });

            IHost host = builder.Build();

            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}