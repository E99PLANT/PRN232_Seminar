namespace UserBehaviorService.Application.MessagingContracts
{
    public class RawEventsResponseMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
        public List<EventItemMessage> Events { get; set; } = new();
    }
}
