namespace EventStore.CosmosDb
{
    /// <summary>
    /// Provides access to configured Cosmos DB appsettings using options.
    /// </summary>
    public class CosmosClientOptions
    {
        public CosmosClientOptions() 
        {
            AccountEndpoint = string.Empty;
            Database = string.Empty;
            Containers = new CosmosClientContainersOptions();
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
        public CosmosClientContainersOptions Containers { get; set; }
    }

    /// <summary>
    /// Provides access to configured Cosmos DB container appsettings using options.
    /// </summary>
    public class CosmosClientContainersOptions
    {
        public CosmosClientContainersOptions()
        {
            TestEvents = string.Empty;
        }

        /// <summary>
        /// The configured TestEvents Cosmos DB container.
        /// </summary>
        public string TestEvents { get; set; }
    }
}
