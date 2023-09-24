using Azure.Identity;
using CommonFunctionality.AppToken;
using CommonFunctionality.CosmosDbProvider;
using CommonFunctionality.Model;
using CommonFunctionality.StorageAccount;
using DotNetCoreSqlDb.Models;
using FoodInspector.Configuration;
using FoodInspector.InspectionDataGatherer;
using FoodInspector.InspectionDataWriter;
using FoodInspector.Providers.EstablishmentsProvider;
using FoodInspector.Providers.EstablishmentsTableProvider;
using FoodInspector.Providers.ExistingInspectionsTableProvider;
using FoodInspector.Providers.SQLDatabaseProvider;
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

            // The ConfigureWebJobs extension method initializes the WebJobs host
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddTimers();
            });

            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddEnvironmentVariables();

                // Load the appropriate appsettings*.json file, depending on the environment
                string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                configurationBuilder.AddJsonFile($"appsettings.json");
                configurationBuilder.AddJsonFile($"appsettings.{environment}.json");

                var configRoot = configurationBuilder.Build();

                // appsettings*.json contains the Key Vault name, so add the Key Vault to the configuration
                configurationBuilder.AddAzureKeyVault(
                    new Uri($"https://{configRoot["KeyVault:VaultName"]}.vault.azure.net/"),
                    new DefaultAzureCredential());

                var config = configurationBuilder.Build();
            });

            // Configure the Dependency Injection container
            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ILoggerFactory, LoggerFactory>();
                services.AddSingleton<IInspectionDataWriter, FoodInspector.InspectionDataWriter.InspectionDataWriter>();
                services.AddSingleton<ICommonServiceLayerProvider, CommonServiceLayerProvider>();
                services.AddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider>();
                services.AddSingleton<IEstablishmentsTableProvider, Providers.EstablishmentsTableProvider.ExistingInspectionsTableProvider>();
                services.AddSingleton<IInspectionDataGatherer, InspectionDataGatherer.InspectionDataGatherer>();
                services.AddSingleton<IEstablishmentsProvider, EstablishmentsProvider>();
                services.AddSingleton<ICosmosDbProviderFactory<InspectionData>, InspectionDataCosmosDbProviderFactory>();
                services.AddSingleton<IExistingInspectionsTableProvider, Providers.ExistingInspectionsTableProvider.ExistingInspectionsTableProvider>();
                services.AddDbContext<FoodInspectorDatabaseContext>(options =>
                    options.UseSqlServer(configurationManager.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));
                services.AddLogging();

                AddOptions(services, hostContext.Configuration);
            });

            // The AddConsole method adds console logging to the configuration
            builder.ConfigureLogging((context, b) =>
            {
                b.SetMinimumLevel(LogLevel.Information);
                b.AddConsole();
                b.AddApplicationInsightsWebJobs(o => { o.ConnectionString = configurationManager.GetConnectionString("AzureWebJobsDashboard"); });
            });

            IHost host = builder.Build();

            using (host)
            {
                await host.RunAsync();
            }
        }

        /// <summary>
        /// Configures IOption instances for dependent service settings.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure options for.</param>
        /// <param name="configRoot">The current <see cref="IConfiguration"/> to use when setting options.</param>
        private static void AddOptions(IServiceCollection services, IConfiguration configRoot)
        {
            // Bind each configuration section and add it to the dependency injection service container
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0
            services.Configure<CosmosDbOptions>(
                configRoot.GetSection("CosmosDb"));
            services.Configure<KeyVaultOptions>(
                configRoot.GetSection("KeyVault"));
            services.Configure<StorageAccountOptions>(
                configRoot.GetSection("Storage"));
            services.Configure<AppTokenOptions>(
                configRoot.GetSection("AppToken"));
        }
    }
}