using CommonFunctionality.Model;

namespace FoodInspector.InspectionDataGatherer
{
    public interface IInspectionDataGatherer
    {
        Task<List<InspectionData>> GatherData();
    }
}