namespace UserBehaviorService.Domain.Entities
{
    public class UserLoginHistory
    {
        public long Id { get; set; }

        public string UserId { get; set; } = default!;
        public string? Email { get; set; }

        public DateTimeOffset LoggedInAt { get; set; }

        public int LoginHour { get; set; }
        public string Weekday { get; set; } = default!;
        public DateTime DateOnlyUtc { get; set; }

        public int SessionDurationMinutes { get; set; }
    }
}
