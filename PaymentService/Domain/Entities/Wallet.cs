namespace PaymentService.Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; }

    // FK → Account (quan hệ 1-1)
    public Guid AccountId { get; set; }

    // Số dư hiện tại
    public decimal Balance { get; set; } = 0;

    // Loại tiền tệ
    public string Currency { get; set; } = "VND";

    // Cập nhật lần cuối
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation
    public Account Account { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
