using HttpClientTest.Model;

namespace HttpClientTest.HttpHelpers
{
    public interface ICommonServiceLayerProvider
    {
        Task<List<FoodInspector.Model.InspectionData>> GetInspections(string name, string city, string date);
    }
}