using FoodInspectorModels;

namespace FoodInspector.Providers.AzureAIProvider
{
    public interface IAzureAIProvider
    {
        public string Check(List<InspectionRecordOpenAIRequestModel> inspectionRecordOpenAIRequestModels);
    }
}
