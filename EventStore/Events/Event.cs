using EventStore.Domain;
using System.Text.Json;

namespace EventStore.Events
{
    public class Event<TDomainModel, TDataType> : Event<TDomainModel> where TDomainModel : IDomainModel
    {
        private TDataType? eventData;
        public TDataType EventData
        {
            get
            {
                if (eventData == null)
                {
                    eventData = JsonSerializer.Deserialize<TDataType>(Data) ?? throw new InvalidCastException();
                }
                return eventData;
            }
            private init => eventData = value;
        }
        public Event() { }
        public Event(Event<TDomainModel> e) : base(e.id, e.StreamId, e.EventNumber, e.Created, e.EventType, e.OriginatingComponent, e.OriginatingComponentVersion)
        {

            eventData = JsonSerializer.Deserialize<TDataType>(e.Data) ?? throw new InvalidCastException();
            Data = e.Data;
        }
        public Event(string id, string streamId, long eventNumber, DateTime created, string eventType, string originatingComponent, string originatingComponentVersion, TDataType data) : base(id, streamId, eventNumber, created, eventType, originatingComponent, originatingComponentVersion)
        {
            Data = JsonSerializer.SerializeToUtf8Bytes(data);
            eventData = data;
        }
    }

    public class Event<TDomainModel> where TDomainModel : IDomainModel
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string StreamId { get; set; } = string.Empty;
        public long EventNumber { get; set; }
        public DateTime Created { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string OriginatingComponent { get; set; } = string.Empty;
        public string OriginatingComponentVersion { get; set; } = string.Empty;
        public byte[] Data { get; init; } = Array.Empty<byte>();
        public Dictionary<string, string> Headers { get; set; } = new();

        // ************************ DO NOT CHANGE THIS VALUE ***********************
        // Changing this version is only required in response to a breaking
        // change to the base Event schema. If such a change is required,
        // A new implementation such as EventV2<TDomainModel> should be implemented
        private static Version version { get; } = new Version(1, 0, 0);
        // *************************************************************************
        public Version Version { get => version; init { if (value != version) throw new Exception(); } }// EventVersionMismatchException(version, value); } }

        public Event() { }

        public Event(string id, string streamId, long eventNumber, DateTime created, string eventType, string originatingComponent, string originatingComponentVersion)
        {
            this.PartitionKey = streamId;
            this.id = id;
            this.StreamId = streamId;
            this.EventNumber = eventNumber;
            this.Version = version;
            this.Created = created;
            this.EventType = eventType;
            this.Headers = new Dictionary<string, string>();
            this.OriginatingComponent = originatingComponent;
            this.OriginatingComponentVersion = originatingComponentVersion;
        }

        public virtual TDomainModel ApplyEvent(TDomainModel model)
        {
            throw new NotImplementedException();
        }
    }
}
