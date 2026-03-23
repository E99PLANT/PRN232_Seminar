namespace UserProfileService.Infrastructure.Messaging.Contracts
{
    public class UserProfileReplayResponseMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
        public string? Email { get; set; }
        public int ProfileUpdateCount { get; set; }
        public DateTimeOffset? LastProfileUpdatedAt { get; set; }
    }
}
