using System.Security.Policy;

namespace LatestInspectionsProcessor.Providers.AzureAIProvider
{
    public class AzureAIOptions
    {
        public AzureAIOptions()
        {
            Endpoint = string.Empty;
            Deployment = string.Empty;
            Question = string.Empty;
            SystemMessageFileName = string.Empty;
            JsonSchemaFileName = string.Empty;
            SystemMessageBlobContainer = string.Empty;
        }

        /// <summary>
        /// The access key for the InspectionResults Event Grid Topic.
        /// </summary>
        public string Endpoint { get; set; }
        public string Deployment {  get; set; }
        public string Question { get; set; }
        public string SystemMessageFileName {  get; set; }
        public string JsonSchemaFileName { get; set; }
        public string SystemMessageBlobContainer { get; set; }
    }
}
