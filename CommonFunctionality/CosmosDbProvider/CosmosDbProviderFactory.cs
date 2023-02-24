namespace CommonFunctionality.CosmosDbProvider
{
    public abstract class CosmosDbProviderFactory<T> : ICosmosDbProviderFactory<T> where T : CosmosDbDocument
    {
        protected abstract ICosmosDbProvider<T> MakeProvider();

        public ICosmosDbProvider<T> CreateProvider()
        {
            return MakeProvider();
        }
    }
}
