namespace PRN232_Seminar.Shared.Events;

public record StockReservationFailedEvent
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public string Reason { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}