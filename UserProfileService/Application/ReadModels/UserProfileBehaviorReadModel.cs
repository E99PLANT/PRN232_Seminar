namespace UserProfileService.Application.ReadModels
{
    public class UserProfileBehaviorReadModel
    {
        public string UserId { get; set; } = default!;
        public string? Email { get; set; }
        public int ProfileUpdateCount { get; set; }
        public DateTimeOffset? LastProfileUpdatedAt { get; set; }
    }
}
