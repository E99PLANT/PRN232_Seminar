namespace PaymentService.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = default!;
    public string EventData { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? Error { get; set; }
}
