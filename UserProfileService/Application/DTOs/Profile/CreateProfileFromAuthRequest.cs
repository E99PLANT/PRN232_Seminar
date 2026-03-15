using System.ComponentModel.DataAnnotations;

namespace UserProfileService.Application.DTOs.Profile
{
    public class CreateProfileFromAuthRequest
    {
        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
    }
}
