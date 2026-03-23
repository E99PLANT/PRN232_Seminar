namespace AuthService.Infrastructure.Messaging.Contracts
{
    public class AuthReplayResponseMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
        public string? Email { get; set; }
        public string CurrentStatus { get; set; } = "Pending";
        public DateTimeOffset? RegisteredAt { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public int LoginCount { get; set; }
        public DateTimeOffset? FirstLoginAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public int LockedCount { get; set; }
        public int UnlockedCount { get; set; }
        public DateTimeOffset? LastLockedAt { get; set; }
        public DateTimeOffset? LastUnlockedAt { get; set; }
        public int PreferredLoginHour { get; set; }
        public string? MostActiveWeekday { get; set; }
        public double AverageDaysBetweenLogins { get; set; }
        public int EstimatedActiveDaysSpan { get; set; }
        public double AverageSessionDurationMinutes { get; set; }
    }
}
