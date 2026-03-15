namespace UserBehaviorService.Application.ReadModels
{
    public class UserBehaviorReadModel
    {
        public string UserId { get; set; } = default!;
        public string? Email { get; set; }
        public string CurrentStatus { get; set; } = "Pending";

        public int LoginCount { get; set; }
        public DateTimeOffset? FirstLoginAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }

        public int ProfileUpdateCount { get; set; }
        public DateTimeOffset? LastProfileUpdatedAt { get; set; }

        public int PreferredLoginHour { get; set; }
        public string? MostActiveWeekday { get; set; }

        public double? AverageDaysBetweenLogins { get; set; }
        public int EstimatedActiveDaysSpan { get; set; }
        public double? AverageSessionDurationMinutes { get; set; }
    }
}
