using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces
{
    public interface IOrderAppService
    {
        // 1. Hàm tạo đơn hàng
        Task<Guid> CreateOrderAsync(CreateOrderRequestDto request);

        // 2. Hàm lấy 1 đơn hàng (Để Controller không bị lỗi)
        Task<Order?> GetOrderAsync(Guid orderId);

        // 3. Hàm lấy toàn bộ đơn hàng
        Task<IEnumerable<Order>> GetAllOrdersAsync();
    }
}