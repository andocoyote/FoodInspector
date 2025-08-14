using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using CommonFunctionality.CosmosDbProvider;
using CommonFunctionality.StorageAccount;
using FoodInspector.Configuration;
using FoodInspector.Providers.ExistingInspectionsTableProvider;
using FoodInspector.Providers.InspectionDataGatherer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

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
                string foodInspectorApiUri = hostContext.Configuration["FoodInspectorApi:Uri"];

                AddOptions(services, hostContext.Configuration);

                services.AddSingleton<ILoggerFactory, LoggerFactory>();
                services.AddSingleton<ICosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument>, InspectionDataCosmosDbProviderFactory>();
                services.AddSingleton<IExistingInspectionsTableProvider, ExistingInspectionsTableProvider>();

                // Register HttpClient with DI, pre-configured for Easy Auth using Workflow Identity Federation
                // https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-config-app-trust-managed-identity?tabs=microsoft-entra-admin-center%2Cdotnet
                // https://learn.microsoft.com/en-us/azure/app-service/configure-authentication-provider-aad?tabs=workforce-configuration
                var tokenExhangeScope = hostContext.Configuration["FoodInspectorApi:TokenExchangeScope"];
                var apiTenantId = hostContext.Configuration["Api:TenantId"];
                var managedIdentityClientId = hostContext.Configuration["Api:ManagedIdentityClientId"];
                var appRegistrationClientId = hostContext.Configuration["Api:AppRegistrationClientId"];
                var apiScope = hostContext.Configuration["Api:ApiScope"];

                services.AddHttpClient("InspectionDataGatherer", client =>
                {
                    var baseUrl = foodInspectorApiUri;
                    if (string.IsNullOrEmpty(baseUrl))
                        throw new InvalidOperationException("foodInspectorApiUri not configured.");

                    client.BaseAddress = new Uri(baseUrl);
                })
                .AddHttpMessageHandler(sp =>
                {
                    ManagedIdentityCredential miCredential = new (
                        ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId));

                    return new AuthHeaderHandler(
                        miCredential,
                        tokenExhangeScope,
                        apiTenantId,
                        appRegistrationClientId,
                        apiScope
                    );
                });

                // Register your services that use the named HttpClient
                services.AddSingleton<IInspectionDataGatherer, InspectionDataGatherer>();

                string serviceBusNamespace = hostContext.Configuration["ServiceBus:ServiceBusNamespace"];
                string queueName = hostContext.Configuration["ServiceBus:QueueName"];

                // Register the Service Bus Client
                services.AddSingleton(serviceProvider =>
                {
                    return new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential());
                });

                // Register the Service Bus Sender
                services.AddSingleton(serviceProvider =>
                {
                    var client = serviceProvider.GetRequiredService<ServiceBusClient>();
                    return client.CreateSender(queueName);
                });

                services.AddLogging();
            });

            // The AddConsole method adds console logging to the configuration
            builder.ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
                loggingBuilder.AddConsole();
                loggingBuilder.AddApplicationInsightsWebJobs(o => { o.ConnectionString = configurationManager.GetConnectionString("AzureWebJobsDashboard"); });
                loggingBuilder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning); // Suppress HttpClient info logs
                loggingBuilder.AddFilter("Azure.Core", LogLevel.Warning); // Suppress Azure SDK info logs
                loggingBuilder.AddFilter("Azure.Identity", LogLevel.Warning);
                loggingBuilder.AddFilter("Azure.Storage", LogLevel.Warning);
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
            services.Configure<ApiOptions>(
                configRoot.GetSection("Api"));
        }

        // Custom DelegatingHandler to attach Bearer token
        public class AuthHeaderHandler : DelegatingHandler
        {
            private readonly TokenCredential _credential;
            private readonly string _tokenExhangeScope;
            private readonly string _apiTenantId;
            private readonly string _appRegistrationClientId;
            private readonly string _apiScope;

            public AuthHeaderHandler(
                TokenCredential credential,
                string tokenExhangeScope,
                string apiTenantId,
                string appRegistrationClientId,
                string apiScope)
            {
                _credential = credential ?? throw new ArgumentNullException(nameof(credential));
                _tokenExhangeScope = tokenExhangeScope ?? throw new ArgumentNullException(nameof(tokenExhangeScope));
                _apiTenantId = apiTenantId ?? throw new ArgumentNullException(nameof(_apiTenantId));
                _appRegistrationClientId = appRegistrationClientId ?? throw new ArgumentNullException(nameof(appRegistrationClientId));
                _apiScope = apiScope ?? throw new ArgumentNullException(nameof(apiScope));
            }

            // Uses Workflow Identity Federation to call the FoodInspectorAPIs:
            //  1. Get the Managed Identity token
            //  2. Exchange that token for an Azure resource token
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                TokenRequestContext tokenRequestContext = new([_tokenExhangeScope]);
                ClientAssertionCredential clientAssertionCredential = new(
                    _apiTenantId,
                    _appRegistrationClientId,
                    async _ =>
                        (await _credential
                            .GetTokenAsync(tokenRequestContext, cancellationToken)
                            .ConfigureAwait(false)).Token
                );

                TokenRequestContext tokenRequestContext2 = new([_apiScope]);
                var token = clientAssertionCredential.GetToken(tokenRequestContext2, cancellationToken);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                return await base.SendAsync(request, cancellationToken);
            }
        }

    }
}