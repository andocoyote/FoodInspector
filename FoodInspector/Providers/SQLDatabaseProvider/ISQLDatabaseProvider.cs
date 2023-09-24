using CommonFunctionality.Model;

namespace FoodInspector.Providers.SQLDatabaseProvider
{
    public interface ISQLDatabaseProvider
    {
        public string ConnectionString { get; set; }

        void WriteRecord(InspectionData inspectionData);
    }
}