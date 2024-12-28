using Azure.Identity;
using CommonFunctionality.CosmosDbProvider;
using LatestInspectionsProcessor.Providers.AzureAIProvider;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = new HostBuilder();

IConfigurationRoot? configRoot = null;

// Configure configuration variables
builder.ConfigureAppConfiguration(configurationBuilder =>
{
    configurationBuilder
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();

    configRoot = configurationBuilder.Build();

    // appsettings*.json contains the Key Vault name, so add the Key Vault to the configuration
    configurationBuilder.AddAzureKeyVault(
        new Uri($"https://{configRoot["KeyVault:VaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());

    configRoot = configurationBuilder.Build();
});

// Configure services
builder.ConfigureFunctionsWebApplication().ConfigureServices(services =>
{
    services.AddOptions<CosmosDbOptions>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection("CosmosDb").Bind(settings);
    });

    services.AddOptions<AzureAIOptions>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection("AzureAI").Bind(settings);
    });

    if (configRoot != null)
    {
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(new Uri($"https://{configRoot["Storage:StorageAccountName"]}.blob.core.windows.net/"));
        });
    }

    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();

    services.AddSingleton<ILoggerFactory, LoggerFactory>();
    services.AddSingleton<ICosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument>, InspectionDataCosmosDbProviderFactory>();
    services.AddSingleton<IAzureAIProvider, AzureAIProvider>();
});

// The AddConsole method adds console logging to the configuration
builder.ConfigureLogging((context, b) =>
{
    b.SetMinimumLevel(LogLevel.Information);
    b.AddConsole();
});

IHost host = builder.Build();

using (host)
{
    await host.RunAsync();
}