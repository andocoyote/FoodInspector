using FoodInspector.StorageTableProvider;
using HttpClientTest.Model;

namespace HttpClientTest.HttpHelpers
{
    public interface ICommonServiceLayerProvider
    {
        Task<List<FoodInspector.Model.InspectionData>> GetInspections(List<EstablishmentsModel> establishmentsModels);
    }
}