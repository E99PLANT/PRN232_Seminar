using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Auth
{
    public class LockAccountRequest
    {
        [Required]
        public string Reason { get; set; } = default!;
    }
}
