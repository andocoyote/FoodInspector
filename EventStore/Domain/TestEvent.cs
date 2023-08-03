namespace EventStore.Domain
{
    using System;

    public class TestEvent
    {
        public Guid id { get; set; }
        public string Owner { get; set; }
        public long Longnumber { get; set; }
        public DateTime SomeDateTime { get; set; }

        public TestEvent()
        { }

        public TestEvent(Guid id, string owner, long longnumber, DateTime somedatetime)
        {
            this.id = id;
            Owner = owner;
            Longnumber = longnumber;
            SomeDateTime = somedatetime;
        }
    }
}
