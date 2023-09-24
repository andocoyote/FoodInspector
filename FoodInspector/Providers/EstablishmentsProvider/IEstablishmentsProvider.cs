namespace FoodInspector.Providers.EstablishmentsProvider
{
    public interface IEstablishmentsProvider
    {
        List<EstablishmentsModel> ReadEstablishmentsFile();
    }
}