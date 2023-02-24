using Microsoft.Azure.Cosmos;

namespace CommonFunctionality.CosmosDbProvider
{
    public interface ICosmosDbProvider<T> where T : CosmosDbDocument
    {
        Task WriteDocument(T document);
        Task<T> ReadDocument(string id, PartitionKey partitionKey);
    }
}