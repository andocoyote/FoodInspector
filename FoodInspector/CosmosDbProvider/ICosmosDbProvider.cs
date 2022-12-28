using FoodInspector.Model;

namespace FoodInspector.CosmosDbProvider
{
    public interface ICosmosDbProvider
    {
        Task WriteDocument(InspectionData inspectionData);
    }
}