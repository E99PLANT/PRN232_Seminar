namespace UserBehaviorService.Application.MessagingContracts
{
    public class RawEventsRequestMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
    }
}
