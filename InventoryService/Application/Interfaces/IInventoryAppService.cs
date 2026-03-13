using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces
{
    public interface IInventoryAppService
    {
        // CRUD CƠ BẢN CHO ADMIN
        Task<IEnumerable<Inventory>> GetAllStockAsync();
        Task<Inventory?> GetStockAsync(Guid productId);
        Task<Inventory> ImportStockAsync(Guid productId, string productName, int quantity);
        Task<bool> DeleteStockAsync(Guid productId);

        // LẤY LỊCH SỬ SỰ KIỆN CHO MÀN HÌNH EVENT MONITOR
        Task<IEnumerable<InventoryEvent>> GetRecentEventsAsync(int count = 10);

        // XỬ LÝ NGHIỆP VỤ LÕI CỦA SAGA (Gọi từ RabbitMQ Consumer)
        Task<(bool IsSuccess, string Reason)> ProcessOrderCreatedAsync(Guid orderId, Guid productId, int quantity);
    }
}