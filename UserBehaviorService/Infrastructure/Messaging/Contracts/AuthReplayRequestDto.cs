namespace UserBehaviorService.Infrastructure.Messaging.Contracts
{
    public class AuthReplayRequestDto
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
    }
}
