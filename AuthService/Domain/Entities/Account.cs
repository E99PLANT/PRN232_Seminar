using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public AccountStatus Status { get; set; } = AccountStatus.Pending;

        public string? RegisterOtpHash { get; set; }
        public DateTimeOffset? RegisterOtpExpiresAt { get; set; }
        public int RegisterOtpFailCount { get; set; }
        public DateTimeOffset? RegisterOtpVerifiedAt { get; set; }

        public ICollection<EventStoreRecord> Events { get; set; } = new List<EventStoreRecord>();
    }
}
