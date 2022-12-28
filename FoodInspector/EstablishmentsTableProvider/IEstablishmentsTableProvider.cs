using FoodInspector.EstablishmentsProvider;

namespace FoodInspector.EstablishmentsTableProvider
{
    public interface IEstablishmentsTableProvider
    {
        Task CreateEstablishmentsSet();

        Task<List<EstablishmentsModel>> GetEstablishmentsSet();
    }
}