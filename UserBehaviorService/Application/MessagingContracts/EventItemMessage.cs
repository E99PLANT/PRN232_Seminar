namespace UserBehaviorService.Application.MessagingContracts
{
    public class EventItemMessage
    {
        public string EventType { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }
        public string PayloadJson { get; set; } = "{}";
    }
}
