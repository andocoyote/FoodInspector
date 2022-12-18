using Azure;
using Azure.Data.Tables;

namespace FoodInspector.StorageTableProvider
{
    public class EstablishmentsModel : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
