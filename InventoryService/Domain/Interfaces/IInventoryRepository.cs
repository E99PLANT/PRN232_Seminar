using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Interfaces
{
    public interface IInventoryRepository
    {
        // --- CRUD CƠ BẢN ---
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory?> GetInventoryByProductIdAsync(Guid productId);
        Task AddAsync(Inventory inventory);
        Task UpdateInventoryAsync(Inventory inventory);
        Task DeleteAsync(Inventory inventory);

        // --- EVENT SOURCING ---
        Task<IEnumerable<InventoryEvent>> GetRecentEventsAsync(int count = 10);
        Task AppendEventAsync(InventoryEvent inventoryEvent); // Để ghi log vào Radar FE
        Task SaveChangesAsync(); // Để chốt lệnh lưu vào DB

    }
}