using CommonFunctionality.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommonFunctionality.CosmosDbProvider
{
    public class InspectionDataCosmosDbProviderFactory : CosmosDbProviderFactory<InspectionData>
    {
        private readonly IOptions<CosmosDbOptions> _cosmosDbOptions;
        private readonly ILoggerFactory _loggerFactory;

        public InspectionDataCosmosDbProviderFactory(
            IOptions<CosmosDbOptions> cosmosDbOptions,
            ILoggerFactory loggerFactory)
        {
            _cosmosDbOptions = cosmosDbOptions;
            _loggerFactory = loggerFactory;
        }
        protected override ICosmosDbProvider<InspectionData> MakeProvider()
        {
            ICosmosDbProvider<InspectionData> provider = new InspectionDataCosmosDbProvider(
                _cosmosDbOptions,
                _loggerFactory);

            return provider;
        }
    }
}
