namespace PRN232_Seminar.Shared.Events;

public record InventoryReservedEvent
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}