namespace UserProfileService.Application.DTOs.Profile
{
    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
    }
}
