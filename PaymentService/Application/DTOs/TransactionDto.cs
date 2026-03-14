namespace PaymentService.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSuspicious { get; set; }
    public string? SuspiciousReason { get; set; }
}
