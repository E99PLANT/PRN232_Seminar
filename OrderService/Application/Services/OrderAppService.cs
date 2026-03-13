using MassTransit;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using PRN232_Seminar.Shared.Events;
using System.Text.Json;

namespace OrderService.Application.Services
{
    public class OrderAppService : IOrderAppService
    {
        private readonly IOrderRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderAppService(IOrderRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> CreateOrderAsync(CreateOrderRequestDto request)
        {
            // 1. Tạo đơn hàng với trạng thái Pending (Chờ xử lý)
            var order = new Order
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            // 2. Áp dụng Event Sourcing: Tạo log sự kiện OrderCreated
            var orderEvent = new OrderEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = order.Id,
                AggregateType = "Order",
                EventType = "OrderCreated",
                EventData = JsonSerializer.Serialize(new { ProductId = order.ProductId, Quantity = order.Quantity }),
                Timestamp = DateTime.UtcNow
            };

            // 3. Lưu cả Đơn hàng và Lịch sử sự kiện vào DB
            await _repository.AddOrderAsync(order);
            await _repository.AppendEventAsync(orderEvent);
            await _repository.SaveChangesAsync();

            // ============================================================
            // 4. BẮN SỰ KIỆN LÊN RABBITMQ (ĐIỂM CHỐT CỦA MICROSERVICES)
            // ============================================================
            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Timestamp = DateTime.UtcNow
            });

            return order.Id;
        }

        public async Task<Order?> GetOrderAsync(Guid orderId)
        {
            return await _repository.GetOrderByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _repository.GetAllOrdersAsync();
        }
    }
}