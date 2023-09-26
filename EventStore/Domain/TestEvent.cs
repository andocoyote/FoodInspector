namespace EventStore.Domain
{
    using System;

    public class TestEvent : IDomainModel
    {
        public string id { get; set; }
        public string Owner { get; set; }
        public long Longnumber { get; set; }
        public DateTime SomeDateTime { get; set; }
        public string StreamId { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }

        public bool IsFailed => throw new NotImplementedException();

        public bool IsCancelled => throw new NotImplementedException();

        public bool IsSucceeded => throw new NotImplementedException();

        public TestEvent()
        { }

        public TestEvent(string id, string owner, long longnumber, DateTime somedatetime)
        {
            this.id = id;
            Owner = owner;
            Longnumber = longnumber;
            SomeDateTime = somedatetime;
        }
    }
}
