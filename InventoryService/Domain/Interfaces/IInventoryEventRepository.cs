using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Interfaces
{
    public interface IInventoryRepository
    {
        // Lấy thông tin tồn kho hiện tại của một sản phẩm
        Task<Inventory?> GetInventoryByProductIdAsync(Guid productId);

        // Thêm một sự kiện mới vào cuốn sổ cái (Event Store)
        Task AppendEventAsync(InventoryEvent inventoryEvent);

        // Cập nhật số lượng tồn kho (Read Model)
        Task UpdateInventoryAsync(Inventory inventory);

        Task<IEnumerable<InventoryEvent>> GetRecentEventsAsync(int count = 10);

        // Lưu thay đổi vào DB (dùng chung 1 transaction của EF Core)
        Task SaveChangesAsync();
    }
}