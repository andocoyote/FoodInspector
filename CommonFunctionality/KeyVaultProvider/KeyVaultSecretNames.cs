namespace CommonFunctionality.KeyVaultProvider
{
    public static class KeyVaultSecretNames
    {
        public static string ServicePrincipalClientID { get; } = "FoodInspector-ServicePrincipal-ClientID";
        public static string ServicePrincipalTokenSecret { get; } = "FoodInspector-ServicePrincipal-TokenSecret";
        public static string ServicePrincipalTenantID { get; } = "FoodInspector-ServicePrincipal-TenantID";
        public static string ServicePrincipalAppIDURI { get; } = "FoodInspector-ServicePrincipal-AppIDURI";
        public static string FoodEstablishmentInspectionDataAppToken { get; } = "AppToken-King-County-Food-Establishment-Inspection-Data";
        public static string stfoodinspectorConnectionString { get; } = "Storage-stfoodinspector-ConnectionString";
        public static string stfoodinspectorKey { get; } = "Storage-stfoodinspector-Key";
        public static string sqlGeneralStorageUsername { get; } = "SQL-sql-general-storage-Username";
        public static string sqlGeneralStoragePassword { get; } = "SQL-sql-general-storage-Password";
        public static string cosmosfoodinspectorConnectionString { get; } = "CosmosDB-cosmos-food-inspector-ConnectionString";
        public static string eventGridTopicInspectionResultsKey { get; } = "EventGridTopic-InspectionResults-Key";
    }
}
