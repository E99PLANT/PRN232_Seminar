namespace PaymentService.Application.DTOs;

public class WalletDto
{
    public Guid WalletId { get; set; }
    public Guid AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime LastUpdated { get; set; }
}
