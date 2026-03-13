using MassTransit;
using PRN232_Seminar.Shared.Events;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using System.Text.Json;

namespace OrderService.Application.Consumers
{
    public class InventoryReservedConsumer : IConsumer<InventoryReservedEvent>
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<InventoryReservedConsumer> _logger;

        public InventoryReservedConsumer(IOrderRepository repository, ILogger<InventoryReservedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<InventoryReservedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("✅ [RabbitMQ] Tin vui: Kho đã trừ xong cho Đơn hàng {OrderId}", message.OrderId);

            var order = await _repository.GetOrderByIdAsync(message.OrderId);
            if (order == null) return;

            // 1. Chốt đơn hàng
            order.Status = "Confirmed";
            order.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateOrderAsync(order);

            // 2. Ghi sổ cái Event Sourcing
            var orderEvent = new OrderEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = order.Id,
                AggregateType = "Order",
                EventType = "OrderConfirmed",
                EventData = JsonSerializer.Serialize(new { Status = "Success", Notes = "Kho đủ hàng, đã chốt đơn." }),
                Timestamp = DateTime.UtcNow
            };
            await _repository.AppendEventAsync(orderEvent);

            // 3. Lưu xuống Database
            await _repository.SaveChangesAsync();
            _logger.LogInformation("🎉 [Saga] Đã hoàn tất luồng Saga thành công cho Đơn hàng {OrderId}", message.OrderId);
        }
    }
}