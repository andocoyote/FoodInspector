namespace FoodInspector.EstablishmentsProvider
{
    public interface IEstablishmentsProvider
    {
        List<EstablishmentsModel> ReadEstablishmentsFile();
    }
}