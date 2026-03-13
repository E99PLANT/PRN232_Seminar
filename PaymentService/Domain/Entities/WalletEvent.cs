namespace PaymentService.Domain.Entities;

/// <summary>
/// Event Sourcing Entity — Ghi lại mọi thay đổi trạng thái của ví
/// Pattern giống InventoryEvent của InventoryService
/// </summary>
public class WalletEvent
{
    /// <summary>Khóa chính của event</summary>
    public Guid Id { get; set; }

    /// <summary>ID của đối tượng liên quan (WalletId)</summary>
    public Guid AggregateId { get; set; }

    /// <summary>Loại đối tượng: "Wallet"</summary>
    public string AggregateType { get; set; } = "Wallet";

    /// <summary>
    /// Loại sự kiện: 
    /// "WalletCreated", "Deposited", "Withdrawn", "SuspiciousDetected"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Chi tiết sự kiện dạng JSON</summary>
    public string EventData { get; set; } = string.Empty;

    /// <summary>Thời gian xảy ra sự kiện</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
