using Microsoft.Azure.Cosmos;

namespace CommonFunctionality.CosmosDbProvider
{
    public interface ICosmosDbProvider<TWriteDocument, TReadDocument>
        where TWriteDocument : CosmosDbWriteDocument
        where TReadDocument : CosmosDbReadDocument
    {
        Task WriteDocumentAsync(TWriteDocument document);
        Task<TReadDocument> ReadDocumentAsync(string id, PartitionKey partitionKey);
        Task<List<TReadDocument>> QueryLatestInspectionRecordsAsync();
    }
}