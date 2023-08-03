namespace EventStore.Events
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Event<DomainModel> where DomainModel : new()
    {
        public Guid id { get; set; }
        public Guid StreamId { get; set; }
        public long EventNumber { get; set; }
        public string Version { get; set; }
        public DateTime Created { get; set; }
        public string EventType { get; set; }
        public string OriginatingComponent { get; set; }
        public string OriginatingComponentVersion { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public Event()
        { }

        public Event(Guid id, Guid streamId, long eventNumber, Version version, DateTime created, string eventType, string originatingComponent, Version originatingComponentVersion)
        {

            this.id = id;
            this.StreamId = streamId;
            this.EventNumber = eventNumber;
            this.Version = version.ToString();
            this.Created = created;
            this.EventType = eventType;
            this.Data = new Dictionary<string, string>();
            this.OriginatingComponent = originatingComponent;
            this.OriginatingComponentVersion = originatingComponentVersion.ToString();
        }

        public virtual DomainModel ApplyEvent(DomainModel model)
        {
            throw new NotImplementedException();
        }
    }
}
