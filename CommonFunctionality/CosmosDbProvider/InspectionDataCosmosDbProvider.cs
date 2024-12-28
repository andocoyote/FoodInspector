using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommonFunctionality.CosmosDbProvider
{
    public class InspectionDataCosmosDbProvider : CosmosDbProviderBase, ICosmosDbProvider<CosmosDbWriteDocument, CosmosDbReadDocument>
    {
        // The ID of this violation in the set of violations for an establishment
        // If there are multiple violations for an establishment during a single inspection,
        // each violation will be assigned an ID (zero based), e.g. 0, 1, 2, and so on
        // This field is lower case because that's what Cosmos DB requires
        //public string id { get; set; } = null;

        public InspectionDataCosmosDbProvider(
            IOptions<CosmosDbOptions> cosmosDbOptions,
            ILoggerFactory loggerFactory) : base(
                cosmosDbOptions,
                loggerFactory)
        {
        }

        public async Task WriteDocumentAsync(CosmosDbWriteDocument document)
        {
            await WriteDocumentAsync<CosmosDbWriteDocument>(document);
        }

        public async Task<CosmosDbReadDocument> ReadDocumentAsync(string id, PartitionKey partitionKey)
        {
            return await ReadDocumentAsync<CosmosDbReadDocument>(id, partitionKey);
        }

        public async Task<List<CosmosDbReadDocument>> QueryLatestInspectionRecordsAsync()
        {
            // The SQL way
            string cosmosDbQueryStr = @"
                SELECT*
                FROM c
                WHERE c.InspectionDate = (
                    SELECT VALUE MAX(c2.InspectionDate)
                    FROM c c2
                    WHERE c2.ProgramIdentifier = c.ProgramIdentifier)";

            // Query syntax (for documentation purposes)
            /*var matches =
                from c in collection
                where c.InspectionDate == (
                    from c2 in collection
                    where c2.ProgramIdentifier == c.ProgramIdentifier
                    select c2.InspectionDate)
                    .Max()
                select c;*/

            // Method syntax (for documentation purposes)
            /*var cosmosDbQuery = collection
                .Where(c => c.InspectionDate == collection
                    .Where(c2 => c2.ProgramIdentifier == c.ProgramIdentifier)
                    .Max(c2 => c2.InspectionDate));*/

            var queryIterator = GetItemQueryIterator<CosmosDbReadDocument>(cosmosDbQueryStr);

            var mostRecentDocuments = new List<CosmosDbReadDocument>();
            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                mostRecentDocuments.AddRange(response);
            }

            // Output the results
            foreach (var doc in mostRecentDocuments)
            {
                Console.WriteLine(doc);
            }

            return mostRecentDocuments;
        }
    }
}
