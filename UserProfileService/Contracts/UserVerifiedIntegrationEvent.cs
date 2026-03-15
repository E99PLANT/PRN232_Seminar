namespace UserProfileService.Contracts
{
    public class UserVerifiedIntegrationEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}
