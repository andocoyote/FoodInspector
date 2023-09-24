using Azure.Data.Tables;
using Azure;

namespace FoodInspector.Providers.ExistingInspectionsTableProvider
{
    public class ExistingInspectionModel : ITableEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; } = default;

        public ETag ETag { get; set; } = default;
    }
}
