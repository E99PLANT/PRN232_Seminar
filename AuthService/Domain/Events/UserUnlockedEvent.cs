namespace AuthService.Domain.Events
{
    public class UserUnlockedEvent : DomainEvent
    {
        public string UserId { get; set; } = default!;
    }
}
