using System;

namespace InventoryService.Domain.Entities
{
    public class InventoryEvent
    {
        // Khóa chính của mỗi sự kiện được ghi lại
        public Guid Id { get; set; }

        // ID của đối tượng liên quan (Ví dụ: Mã đơn hàng OrderId hoặc Mã sản phẩm ProductId)
        public Guid AggregateId { get; set; }

        // Phân loại đối tượng (Ví dụ: "Inventory", "Order")
        public string AggregateType { get; set; } = string.Empty;

        // Tên hành động/sự kiện (Ví dụ: "StockReserved", "StockReservationFailed")
        public string EventType { get; set; } = string.Empty;

        // Dữ liệu chi tiết của sự kiện (Sẽ lưu dạng JSON trong Supabase)
        public string EventData { get; set; } = string.Empty;

        // Thời gian xảy ra sự kiện
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}