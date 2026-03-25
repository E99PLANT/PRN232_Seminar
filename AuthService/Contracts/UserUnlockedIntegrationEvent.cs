namespace AuthService.Contracts
{
    public class UserUnlockedIntegrationEvent
    {
        public string UserId { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }
    }
}
