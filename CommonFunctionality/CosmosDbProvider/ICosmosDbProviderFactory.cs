namespace CommonFunctionality.CosmosDbProvider
{
    public interface ICosmosDbProviderFactory<T> where T : CosmosDbDocument
    {
        ICosmosDbProvider<T> CreateProvider();
    }
}