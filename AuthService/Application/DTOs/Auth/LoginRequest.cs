using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        //test seminar
        public DateTimeOffset? LoggedInAt { get; set; }

        //test seminar
        public int? SessionDurationMinutes { get; set; }
    }
}
