namespace UserProfileService.Domain.Events
{
    public abstract class DomainEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTimeOffset OccurredOn { get; set; } = DateTimeOffset.UtcNow;
        public string EventType => GetType().Name;
    }
}
