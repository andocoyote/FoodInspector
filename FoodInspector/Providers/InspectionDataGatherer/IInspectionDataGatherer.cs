using FoodInspectorModels;

namespace FoodInspector.Providers.InspectionDataGatherer
{
    public interface IInspectionDataGatherer
    {
        Task<List<InspectionRecordAggregated>> QueryAllInspections();
    }
}