namespace OrderService.Domain.Entities
{
    public class OrderEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AggregateId { get; set; } // Chính là OrderId
        public string AggregateType { get; set; } = "Order"; // Loại thực thể
        public string EventType { get; set; } = string.Empty; // VD: "OrderCreated", "OrderCancelled"
        public string EventData { get; set; } = string.Empty; // Lưu Payload dạng JSONB
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}