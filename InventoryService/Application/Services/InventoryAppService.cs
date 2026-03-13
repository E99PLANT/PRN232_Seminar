using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;
using System.Text.Json;

namespace InventoryService.Application.Services
{
    public class InventoryAppService : IInventoryAppService
    {
        private readonly IInventoryRepository _repository;
        public InventoryAppService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        // --- CÁC HÀM CRUD ---
        public async Task<IEnumerable<Inventory>> GetAllStockAsync()
        {
            // Tùy vào Repository của bạn có hàm GetAll chưa, nếu chưa hãy thêm vào nhé
            return await _repository.GetAllAsync();
        }

        public async Task<Inventory?> GetStockAsync(Guid productId)
        {
            return await _repository.GetInventoryByProductIdAsync(productId);
        }

        public async Task<Inventory> ImportStockAsync(Guid productId, string productName, int quantity)
        {
            var existing = await _repository.GetInventoryByProductIdAsync(productId);
            if (existing != null)
            {
                existing.StockQuantity += quantity;
                existing.LastUpdated = DateTime.UtcNow;
                await _repository.UpdateInventoryAsync(existing);
                await _repository.SaveChangesAsync();
                return existing;
            }
            else
            {
                var newStock = new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ProductName = productName,
                    StockQuantity = quantity,
                    LastUpdated = DateTime.UtcNow
                };
                await _repository.AddAsync(newStock); // Đảm bảo repo bạn có hàm AddAsync
                await _repository.SaveChangesAsync();
                return newStock;
            }
        }

        public async Task<bool> DeleteStockAsync(Guid productId)
        {
            var existing = await _repository.GetInventoryByProductIdAsync(productId);
            if (existing == null) return false;

            await _repository.DeleteAsync(existing); // Đảm bảo repo bạn có hàm Delete
            await _repository.SaveChangesAsync();
            return true;
        }

        // --- HÀM LẤY EVENT ---
        public async Task<IEnumerable<InventoryEvent>> GetRecentEventsAsync(int count = 10)
        {
            return await _repository.GetRecentEventsAsync(count);
        }

        // --- HÀM XỬ LÝ SAGA CHÍNH ---
        public async Task<(bool IsSuccess, string Reason)> ProcessOrderCreatedAsync(Guid orderId, Guid productId, int quantity)
        {
            var inventory = await _repository.GetInventoryByProductIdAsync(productId);

            if (inventory != null && inventory.StockQuantity >= quantity)
            {
                // THÀNH CÔNG
                inventory.StockQuantity -= quantity;
                inventory.LastUpdated = DateTime.UtcNow;
                await _repository.UpdateInventoryAsync(inventory);

                var successEvent = new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    AggregateId = orderId,
                    AggregateType = "Order",
                    EventType = "StockReserved",
                    EventData = JsonSerializer.Serialize(new { ProductId = productId, Quantity = quantity, Status = "Success" }),
                    Timestamp = DateTime.UtcNow
                };
                await _repository.AppendEventAsync(successEvent);
                await _repository.SaveChangesAsync();

                return (true, "Thành công");
            }
            else
            {
                // THẤT BẠI
                var reason = inventory == null ? "Sản phẩm không tồn tại" : "Out of stock";
                var failedEvent = new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    AggregateId = orderId,
                    AggregateType = "Order",
                    EventType = "StockReservationFailed",
                    EventData = JsonSerializer.Serialize(new { ProductId = productId, Reason = reason }),
                    Timestamp = DateTime.UtcNow
                };
                await _repository.AppendEventAsync(failedEvent);
                await _repository.SaveChangesAsync();

                return (false, reason);
            }
        }
    }
}