namespace AuthService.Domain.Events
{
    public class UserLoggedInEvent : DomainEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;

        // thời điểm login thực tế hoặc giả lập để demo
        public DateTimeOffset LoggedInAt { get; set; }

        // thời lượng sử dụng của buổi login, dùng cho seminar/demo
        public int SessionDurationMinutes { get; set; }
    }
}
