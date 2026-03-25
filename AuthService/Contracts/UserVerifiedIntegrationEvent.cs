namespace AuthService.Contracts
{
    public class UserVerifiedIntegrationEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }
    }
}
