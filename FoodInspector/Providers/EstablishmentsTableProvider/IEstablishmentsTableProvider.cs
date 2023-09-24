using FoodInspector.Providers.EstablishmentsProvider;

namespace FoodInspector.Providers.EstablishmentsTableProvider
{
    public interface IEstablishmentsTableProvider
    {
        Task CreateEstablishmentsSet();

        Task<List<EstablishmentsModel>> GetEstablishmentsSet();
    }
}