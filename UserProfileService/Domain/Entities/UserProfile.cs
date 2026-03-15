namespace UserProfileService.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public string AccountId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
    }
}
