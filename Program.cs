using System.Threading.Tasks;
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
            var builder = new HostBuilder();

            // The AddConsole method adds console logging to the configuration
            builder.ConfigureLogging((context, b) =>
            {
                b.AddConsole();
            });

            // The ConfigureWebJobs extension method initializes the WebJobs host
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddTimers();
            });

            var host = builder.Build();

            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}