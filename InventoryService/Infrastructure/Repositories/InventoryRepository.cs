using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        // --- CÁC HÀM MỚI ĐƯỢC BỔ SUNG ĐỂ KHỚP VỚI INTERFACE ---

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _context.Inventory.ToListAsync();
        }

        public async Task AddAsync(Inventory inventory)
        {
            await _context.Inventory.AddAsync(inventory);
        }

        public async Task DeleteAsync(Inventory inventory)
        {
            _context.Inventory.Remove(inventory);
            await Task.CompletedTask; // Remove là hàm đồng bộ, ta bọc Task lại cho đúng Interface
        }

        // --- CÁC HÀM CŨ ĐÃ CÓ ---

        public async Task<Inventory?> GetInventoryByProductIdAsync(Guid productId)
        {
            return await _context.Inventory
                .FirstOrDefaultAsync(x => x.ProductId == productId);
        }

        public async Task AppendEventAsync(InventoryEvent inventoryEvent)
        {
            await _context.InventoryEvents.AddAsync(inventoryEvent);
        }

        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            _context.Inventory.Update(inventory);
            await Task.CompletedTask; // Update trong EF là đồng bộ, ta bọc lại bằng Task
        }

        public async Task<IEnumerable<InventoryEvent>> GetRecentEventsAsync(int count = 10)
        {
            // Lấy danh sách sự kiện, sắp xếp mới nhất lên đầu
            return await _context.InventoryEvents
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}