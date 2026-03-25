namespace UserProfileService.Contracts
{
    public class UserProfileCreatedIntegrationEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }
    }
}
