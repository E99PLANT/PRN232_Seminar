namespace AuthService.Domain.Entities
{
    public class EventStoreRecord
    {
        public long Id { get; set; }
        public string AggregateId { get; set; } = default!;
        public string AggregateType { get; set; } = default!;
        public string EventType { get; set; } = default!;
        public string EventData { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }
        public int Version { get; set; }
    }
}
