namespace AuthService.Domain.Events
{
    public class UserLockedEvent : DomainEvent
    {
        public string UserId { get; set; } = default!;
        public string Reason { get; set; } = default!;
    }
}
