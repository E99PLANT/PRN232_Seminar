namespace UserBehaviorService.Domain.Entities
{
    public class ConsumedMessage
    {
        public long Id { get; set; }
        public string MessageId { get; set; } = default!;
        public string EventType { get; set; } = default!;
        public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
