using FoodInspectorModels;

namespace FoodInspector.InspectionDataGatherer
{
    public interface IInspectionDataGatherer
    {
        Task<List<InspectionRecordAggregated>> QueryAllInspections();
    }
}