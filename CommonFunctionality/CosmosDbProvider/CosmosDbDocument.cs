namespace CommonFunctionality.CosmosDbProvider
{
    public abstract class CosmosDbDocument
    {
        public string id { get; set; } = null;

        public string _etag { get; set; } = null;

        public double _ts { get; set; } = 0;
    }
}
