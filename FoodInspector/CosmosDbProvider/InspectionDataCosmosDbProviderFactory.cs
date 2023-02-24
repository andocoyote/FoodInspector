using CommonFunctionality.KeyVaultProvider;
using FoodInspector.EstablishmentsTableProvider;
using FoodInspector.Model;
using FoodInspector.SQLDatabaseProvider;
using HttpClientTest.HttpHelpers;
using Microsoft.Extensions.Logging;

namespace FoodInspector.CosmosDbProvider
{
    public class InspectionDataCosmosDbProviderFactory : CosmosDbProviderFactory<InspectionData>
    {
        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ILoggerFactory _loggerFactory;

        public InspectionDataCosmosDbProviderFactory(
            IKeyVaultProvider keyVaultProvider,
            ILoggerFactory loggerFactory)
        {
            _keyVaultProvider = keyVaultProvider;
            _loggerFactory = loggerFactory;
        }
        protected override ICosmosDbProvider<InspectionData> MakeProvider()
        {
            ICosmosDbProvider<InspectionData> provider = new InspectionDataCosmosDbProvider(_keyVaultProvider, _loggerFactory);

            return provider;
        }
    }
}
