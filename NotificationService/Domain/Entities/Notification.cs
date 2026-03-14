namespace NotificationService.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string? Message { get; set; }
        public DateTime MessagedAt { get; set; }
    }
}
