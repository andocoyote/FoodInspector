namespace CommonFunctionality.AzureAI
{
    public class AzureAIOptions
    {
        public AzureAIOptions()
        {
            Endpoint = string.Empty;
            Deployment = string.Empty;
            Question = string.Empty;
        }

        /// <summary>
        /// The access key for the InspectionResults Event Grid Topic.
        /// </summary>
        public string Endpoint { get; set; }
        public string Deployment {  get; set; }
        public string Question { get; set; }

    }
}
