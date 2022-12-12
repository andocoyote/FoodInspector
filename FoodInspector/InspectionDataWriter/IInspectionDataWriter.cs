namespace FoodInspector.InspectionDataWriter
{
    public interface IInspectionDataWriter
    {
        public Task UpsertData();
    }
}