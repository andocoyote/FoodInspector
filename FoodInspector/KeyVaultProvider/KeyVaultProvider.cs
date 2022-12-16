using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace FoodInspector.KeyVaultProvider
{
    public class KeyVaultProvider : IKeyVaultProvider
    {
        private readonly string _keyVaultName = "kv-general-key-vault";
        private readonly SecretClient _client = null;
        private readonly ILogger _logger;

        public KeyVaultProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<KeyVaultProvider>();

            // The executing user or app must have an access policy in the Key Vault
            _client = new SecretClient(
                new Uri("https://" + _keyVaultName + ".vault.azure.net"),
                new DefaultAzureCredential());
        }

        // Returns the Application Client ID for the App Registration/Service Principal
        public async Task<string> GetClientID()
        {
            return await GetKeyVaultSecret(KeyVaultSecretNames.ServicePrincipalClientID);
        }

        // Returns the token secret for the App Registration/Service Principal
        public async Task<string> GetTokenSecret()
        {
            return await GetKeyVaultSecret(KeyVaultSecretNames.ServicePrincipalTokenSecret);
        }

        // Returns the Tenant ID for the App Registration/Service Principal
        public async Task<string> GetTenantID()
        {
            return await GetKeyVaultSecret(KeyVaultSecretNames.ServicePrincipalTenantID);
        }

        // Returns the Application ID URI for the App Registration/Service Principal
        public async Task<string> GetAppIDURI()
        {
            return await GetKeyVaultSecret(KeyVaultSecretNames.ServicePrincipalAppIDURI);
        }

        // Returns the Food Establishment Inspection Data app token
        public async Task<string> GetAppToken()
        {
            return await GetKeyVaultSecret(KeyVaultSecretNames.FoodEstablishmentInspectionDataAppToken);
        }

        public async Task<string> GetKeyVaultSecret(string secretname)
        {
            _logger.LogInformation($"[GetKeyVaultSecret]: Attempting to retrieve secret {secretname}");

            Azure.Response<KeyVaultSecret> secret = await _client.GetSecretAsync(secretname);
            string tokenSecret = secret?.Value?.Value;

            return tokenSecret;
        }
    }
}
