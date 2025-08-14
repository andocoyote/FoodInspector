using Azure.Identity;
using InspectionsReporter.Providers.EmailMessageProvider;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add configuration from appsettings.json (if running locally)
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Add environment variables
builder.Configuration.AddEnvironmentVariables();

// appsettings*.json contains the Key Vault name, so add the Key Vault to the configuration
string? keyVaultName = builder.Configuration["KeyVault:VaultName"];

if (!string.IsNullOrEmpty(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

// Bind the configuration to a strongly typed class
builder.Services.Configure<EmailMessageOptions>(builder.Configuration.GetRequiredSection("EmailMessage"));

builder.Services.AddSingleton<IEmailMessageProvider, EmailMessageProvider>();

builder.Services.AddLogging(logging =>
{
    logging.AddApplicationInsights();
    logging.AddConsole();
});

builder.Build().Run();
