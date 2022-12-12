using FoodInspector.InspectionDataWriter;
using FoodInspector.KeyVaultProvider;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Creating WebJobs that target .NET
// https://learn.microsoft.com/en-us/azure/app-service/webjobs-sdk-get-started

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
                //services.AddSingleton<IJobActivator, WebJobActivator>();
                services.AddSingleton<IKeyVaultProvider, FoodInspector.KeyVaultProvider.KeyVaultProvider>();
                services.AddSingleton<ILoggerFactory, LoggerFactory>();
                services.AddSingleton<IInspectionDataWriter, FoodInspector.InspectionDataWriter.InspectionDataWriter>();
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

            // https://github.com/Azure/azure-webjobs-sdk/issues/1887
            // https://blog.tech-fellow.net/2019/09/15/adding-di-package-to-webjobs-running-on-net-core/

            var host = builder.Build();
            System.IServiceProvider Services = host.Services;

            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}