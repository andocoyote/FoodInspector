using CommonFunctionality.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommonFunctionality.CosmosDbProvider
{
    public class InspectionDataCosmosDbProvider : CosmosDbProviderBase, ICosmosDbProvider<InspectionData>
    {
        // The ID of this violation in the set of violations for an establishment
        // If there are multiple violations for an establishment during a single inspection,
        // each violation will be assigned an ID (zero based), e.g. 0, 1, 2, and so on
        // This field is lower case because that's what Cosmos DB requires
        //public string id { get; set; } = null;

        public InspectionDataCosmosDbProvider(
            IOptions<CosmosDbOptions> cosmosDbOptions,
            ILoggerFactory loggerFactory) :base(
                cosmosDbOptions,
                loggerFactory)
        {
        }

        public async Task WriteDocument(InspectionData document)
        {
            await WriteDocument<InspectionData>(document);
        }

        public async Task<InspectionData> ReadDocument(string id, PartitionKey partitionKey)
        {
            return await ReadDocument<InspectionData>(id, partitionKey);
        }
    }
}
