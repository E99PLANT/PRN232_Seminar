using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces
{
    public interface IInventoryAppService
    {
        Task HandleOrderCreatedAsync(OrderCreatedEventDto orderEvent);
        // Lấy số lượng tồn kho hiện tại
        Task<InventoryService.Domain.Entities.Inventory?> GetStockAsync(Guid productId);

        // Lấy lịch sử sự kiện
        Task<IEnumerable<InventoryService.Domain.Entities.InventoryEvent>> GetRecentEventsAsync(int count = 10);
    }
}