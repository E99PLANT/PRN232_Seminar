using System;

namespace InventoryService.Domain.Entities
{
    public class Inventory
    {
        public Guid Id { get; set; }

        // ID của sản phẩm
        public Guid ProductId { get; set; }

        // Tên sản phẩm để hiển thị cho tiện
        public string ProductName { get; set; } = string.Empty;

        // Số lượng tồn kho hiện tại
        public int StockQuantity { get; set; }

        // Cập nhật lần cuối khi nào
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}