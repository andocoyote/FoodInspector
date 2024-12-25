using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommonFunctionality.CosmosDbProvider
{
    public class InspectionDataCosmosDbProviderFactory : CosmosDbProviderFactory<CosmosDbWriteDocument, CosmosDbReadDocument>
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
        protected override ICosmosDbProvider<CosmosDbWriteDocument, CosmosDbReadDocument> MakeProvider()
        {
            ICosmosDbProvider<CosmosDbWriteDocument, CosmosDbReadDocument> provider = new InspectionDataCosmosDbProvider(
                _cosmosDbOptions,
                _loggerFactory);

            return provider;
        }
    }
}
