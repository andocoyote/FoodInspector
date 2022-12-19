using Azure;
using Azure.Data.Tables;

namespace FoodInspector.EstablishmentsProvider
{
    public class EstablishmentsModel : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public DateTimeOffset? Timestamp { get; set; } = default;
        public ETag ETag { get; set; } = default;
    }
}
