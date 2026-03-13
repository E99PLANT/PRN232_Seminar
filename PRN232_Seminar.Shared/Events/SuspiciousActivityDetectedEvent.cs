namespace PRN232_Seminar.Shared.Events;

/// <summary>
/// Event phát ra khi PaymentService phát hiện giao dịch bất thường
/// UserService subscribe để ghi log cảnh báo
/// </summary>
public record SuspiciousActivityDetectedEvent
{
    public Guid WalletId { get; init; }
    public Guid TransactionId { get; init; }
    public string Username { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
