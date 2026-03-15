namespace UserProfileService.Domain.Events
{
    public class UserProfileCreatedEvent : DomainEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
