namespace PaymentService.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }

    // Tên đăng nhập (liên kết logic với UserService)
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation: 1 Account - 1 Wallet
    public Wallet Wallet { get; set; } = null!;
}
