namespace CommonFunctionality.CosmosDbProvider
{
    /// <summary>
    /// Provides access to configured Cosmos DB appsettings using options.
    /// </summary>
    public class CosmosDbOptions
    {
        public CosmosDbOptions() 
        {
            AccountEndpoint = string.Empty;
            Database = string.Empty;
            Containers = new CosmosDbContainersOptions();
        }

        /// <summary>
        /// The configured Cosmos DB account endpoint.
        /// </summary>
        public string AccountEndpoint { get; set; }

        /// <summary>
        /// The configured Cosmos DB database.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// The configured Cosmos DB containers.
        /// </summary>
        public CosmosDbContainersOptions Containers { get; set; }
    }

    /// <summary>
    /// Provides access to configured Cosmos DB container appsettings using options.
    /// </summary>
    public class CosmosDbContainersOptions
    {
        public CosmosDbContainersOptions()
        {
            InspectionData = string.Empty;
        }

        /// <summary>
        /// The configured InspectionData Cosmos DB container.
        /// </summary>
        public string InspectionData { get; set; }
    }
}
