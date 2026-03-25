namespace AuthService.Infrastructure.Messaging.Contracts
{
    public class EventItemMessage
    {
        public string EventType { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }
        public string PayloadJson { get; set; } = "{}";
    }
}
