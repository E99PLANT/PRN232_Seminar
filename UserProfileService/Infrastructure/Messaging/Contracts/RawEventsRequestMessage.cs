namespace UserProfileService.Infrastructure.Messaging.Contracts
{
    public class RawEventsRequestMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
    }
}
