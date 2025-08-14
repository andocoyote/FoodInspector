namespace FoodInspector.Configuration
{
    /// <summary>
    /// Provides settings for calling the FoodInspectorAPI using Easy Auth and Workflow Identity Federation.
    /// </summary>
    public class ApiOptions
    {
        public ApiOptions()
        {
            TenantId = string.Empty;
            AppRegistrationClientId = string.Empty;
            ApiScope = string.Empty;
        }

        /// <summary>
        /// The ID of the tenant containing the FoodInspectorAPI
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The Cliend ID of the User-Assigned Managed Identity
        /// </summary>
        public string ManagedIdentityClientId { get; set; }

        /// <summary>
        /// The Cliend ID of the App Registration of the FoodInspectorAPI
        /// </summary>
        public string AppRegistrationClientId { get; set; }

        /// <summary>
        /// The scope of the FoodInspectorAPI
        /// </summary>
        public string ApiScope { get; set; }
    }
}
