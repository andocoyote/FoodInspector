using HttpClientTest.Model;

namespace FoodInspector.SQLDatabaseProvider
{
    public interface ISQLDatabaseProvider
    {
        public string ConnectionString { get; set; }

        void WriteRecord(FoodInspector.Model.InspectionData inspectionData);
    }
}