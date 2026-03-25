namespace AuthService.Infrastructure.Messaging.Contracts
{
    public class AuthReplayRequestMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
    }
}
