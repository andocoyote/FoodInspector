using FoodInspectorModels;

namespace CommonFunctionality.CosmosDbProvider
{
    public class CosmosDbReadDocument : InspectionRecordAggregated
    {
        public string id { get; set; } = string.Empty;
        public string _etag { get; set; } = String.Empty;

        public double _ts { get; set; } = 0;
    }
}
