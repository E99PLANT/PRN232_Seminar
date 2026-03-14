namespace PaymentService.Domain.Entities;

public class WalletTransaction
{
    public Guid Id { get; set; }

    // FK → Wallet
    public Guid WalletId { get; set; }

    // Loại giao dịch: "Deposit", "Withdraw", "Transfer"
    public string TransactionType { get; set; } = string.Empty;

    // Số tiền giao dịch
    public decimal Amount { get; set; }

    // Số dư trước giao dịch
    public decimal BalanceBefore { get; set; }

    // Số dư sau giao dịch
    public decimal BalanceAfter { get; set; }

    // Mô tả giao dịch
    public string? Description { get; set; }

    // Thời gian giao dịch
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // === Phát hiện hoạt động bất thường ===

    // Đánh dấu giao dịch nghi ngờ bất thường
    public bool IsSuspicious { get; set; } = false;

    // Lý do bị đánh dấu bất thường
    public string? SuspiciousReason { get; set; }

    // Navigation
    public Wallet Wallet { get; set; } = null!;
}
