using FoodInspector.EstablishmentsProvider;

namespace FoodInspector.StorageTableProvider
{
    public interface IStorageTableProvider
    {
        Task CreateEstablishmentsSet();

        Task<List<EstablishmentsModel>> GetEstablishmentsSet();
    }
}