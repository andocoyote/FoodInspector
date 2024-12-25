using Microsoft.Azure.Cosmos;

namespace CommonFunctionality.CosmosDbProvider
{
    public interface ICosmosDbProvider<TWriteDocument, TReadDocument>
        where TWriteDocument : CosmosDbWriteDocument
        where TReadDocument : CosmosDbReadDocument
    {
        Task WriteDocument(TWriteDocument document);
        Task<TReadDocument> ReadDocument(string id, PartitionKey partitionKey);
    }
}