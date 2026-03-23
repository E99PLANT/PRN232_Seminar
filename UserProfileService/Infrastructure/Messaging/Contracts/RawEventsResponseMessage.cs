namespace UserProfileService.Infrastructure.Messaging.Contracts
{
    public class RawEventsResponseMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
        public List<EventItemMessage> Events { get; set; } = new();
    }
}
