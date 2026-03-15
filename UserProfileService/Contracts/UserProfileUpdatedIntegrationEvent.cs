namespace UserProfileService.Contracts
{
    public class UserProfileUpdatedIntegrationEvent
    {
        public string UserId { get; set; } = default!;
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public DateTimeOffset OccurredOn { get; set; }
    }
}
