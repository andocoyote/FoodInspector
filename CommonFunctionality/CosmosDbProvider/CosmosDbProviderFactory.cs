namespace CommonFunctionality.CosmosDbProvider
{
    public abstract class CosmosDbProviderFactory<TWriteDocument, TReadDocument> : ICosmosDbProviderFactory<TWriteDocument, TReadDocument>
        where TWriteDocument : CosmosDbWriteDocument
        where TReadDocument : CosmosDbReadDocument
    {
        protected abstract ICosmosDbProvider<TWriteDocument, TReadDocument> MakeProvider();

        public ICosmosDbProvider<TWriteDocument, TReadDocument> CreateProvider()
        {
            return MakeProvider();
        }
    }
}
