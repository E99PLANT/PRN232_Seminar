namespace UserBehaviorService.Application.MessagingContracts
{
    public class AuthReplayRequestMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
    }
}
