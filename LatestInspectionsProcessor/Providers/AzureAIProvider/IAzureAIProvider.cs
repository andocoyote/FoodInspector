using FoodInspectorModels;

namespace LatestInspectionsProcessor.Providers.AzureAIProvider
{
    public interface IAzureAIProvider
    {
        public Task<string> ProcessInspectionResults(List<InspectionRecordOpenAIRequestModel> inspectionRecordOpenAIRequestModels);
    }
}
