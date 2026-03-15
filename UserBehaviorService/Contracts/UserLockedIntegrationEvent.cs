namespace UserBehaviorService.Contracts
{
    public class UserLockedIntegrationEvent
    {
        public string UserId { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public DateTimeOffset OccurredOn { get; set; }
    }
}
