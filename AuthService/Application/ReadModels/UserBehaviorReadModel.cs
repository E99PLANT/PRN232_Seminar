namespace AuthService.Application.ReadModels
{
    public class UserBehaviorReadModel
    {
        public string UserId { get; set; } = default!;
        public string? Email { get; set; }

        public int LoginCount { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }

        public int ProfileUpdateCount { get; set; }

        public string CurrentStatus { get; set; } = "Pending";
    }
}
