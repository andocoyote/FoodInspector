using HttpClientTest.Model;

namespace HttpClientTest.HttpHelpers
{
    public interface ICommonServiceLayerProvider
    {
        Task<List<InspectionData>> GetInspections(string name, string city, string date);
    }
}