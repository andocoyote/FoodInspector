namespace CommonFunctionality.EventGrid
{
    public class EventGridOptions
    {
        public EventGridOptions()
        {
            InspectionResultsKey = string.Empty;
        }

        /// <summary>
        /// The access key for the InspectionResults Event Grid Topic.
        /// </summary>
        public string InspectionResultsKey { get; set; }
    }
}
