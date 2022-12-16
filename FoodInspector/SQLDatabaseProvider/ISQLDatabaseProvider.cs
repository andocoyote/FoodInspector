using HttpClientTest.Model;

namespace FoodInspector.SQLDatabaseProvider
{
    public interface ISQLDatabaseProvider
    {
        void WriteRecord(FoodInspector.Model.InspectionData inspectionData);
    }
}