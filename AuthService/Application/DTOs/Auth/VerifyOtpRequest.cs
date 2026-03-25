using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = default!;
    }
}
