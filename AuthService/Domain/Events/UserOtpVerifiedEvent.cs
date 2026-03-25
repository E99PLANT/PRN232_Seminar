namespace AuthService.Domain.Events
{
    public class UserOtpVerifiedEvent : DomainEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
