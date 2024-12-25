namespace CommonFunctionality.CosmosDbProvider
{
    public interface ICosmosDbProviderFactory<TWriteDocument, TReadDocument>
        where TWriteDocument : CosmosDbWriteDocument
        where TReadDocument : CosmosDbReadDocument
    {
        ICosmosDbProvider<TWriteDocument, TReadDocument> CreateProvider();
    }
}