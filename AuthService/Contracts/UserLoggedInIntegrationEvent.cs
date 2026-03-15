namespace AuthService.Contracts
{
    public class UserLoggedInIntegrationEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }

        public int SessionDurationMinutes { get; set; }
    }
}
