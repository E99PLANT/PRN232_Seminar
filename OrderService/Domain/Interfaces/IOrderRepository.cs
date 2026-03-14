using OrderService.Domain.Entities;

namespace OrderService.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task AddOrderAsync(Order order);
        Task AppendEventAsync(OrderEvent orderEvent);
        Task SaveChangesAsync();
        // --- THÊM 2 HÀM NÀY ĐỂ FRONTEND LẤY DỮ LIỆU ---
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        // --- BỔ SUNG: HÀM NÀY DÙNG ĐỂ CONSUMER CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG ---
        Task UpdateOrderAsync(Order order);
    }
}