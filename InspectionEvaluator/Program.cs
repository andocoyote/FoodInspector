using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = new HostBuilder();

builder.ConfigureFunctionsWebApplication();

builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();
});

builder.ConfigureAppConfiguration(configurationBuilder =>
{
    configurationBuilder
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();

var configRoot = configurationBuilder.Build();

// appsettings*.json contains the Key Vault name, so add the Key Vault to the configuration
configurationBuilder.AddAzureKeyVault(
    new Uri($"https://{configRoot["KeyVault:VaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());

var config = configurationBuilder.Build();
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
