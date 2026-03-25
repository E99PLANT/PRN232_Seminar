namespace UserBehaviorService.Domain.Entities
{
    public class UserBehaviorProjection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string UserId { get; set; } = default!;
        public string? Email { get; set; }

        public string CurrentStatus { get; set; } = "Pending";

        public int LoginCount { get; set; }
        public DateTimeOffset? FirstLoginAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }

        public int ProfileUpdateCount { get; set; }
        public DateTimeOffset? LastProfileUpdatedAt { get; set; }

        public int PreferredLoginHour { get; set; } = -1; // 0..23, -1 = unknown
        public string? MostActiveWeekday { get; set; }

        public double? AverageDaysBetweenLogins { get; set; }
        public int EstimatedActiveDaysSpan { get; set; }

        // để dành cho tương lai nếu có logout/activity event
        public double? AverageSessionDurationMinutes { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
