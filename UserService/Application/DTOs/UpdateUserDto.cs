namespace UserService.Application.DTOs;

public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
}
