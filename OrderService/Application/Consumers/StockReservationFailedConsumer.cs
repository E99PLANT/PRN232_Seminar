using MassTransit;
using PRN232_Seminar.Shared.Events;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using System.Text.Json;

namespace OrderService.Application.Consumers
{
    public class StockReservationFailedConsumer : IConsumer<StockReservationFailedEvent>
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<StockReservationFailedConsumer> _logger;

        public StockReservationFailedConsumer(IOrderRepository repository, ILogger<StockReservationFailedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockReservationFailedEvent> context)
        {
            var message = context.Message;
            _logger.LogWarning("❌ [RabbitMQ] Tin buồn: Kho từ chối Đơn hàng {OrderId}. Lý do: {Reason}", message.OrderId, message.Reason);

            var order = await _repository.GetOrderByIdAsync(message.OrderId);
            if (order == null) return;

            // 1. Hủy đơn hàng (Compensating Transaction)
            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateOrderAsync(order);

            // 2. Ghi sổ cái Event Sourcing
            var orderEvent = new OrderEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = order.Id,
                AggregateType = "Order",
                EventType = "OrderCancelled",
                EventData = JsonSerializer.Serialize(new { Status = "Failed", Reason = message.Reason }),
                Timestamp = DateTime.UtcNow
            };
            await _repository.AppendEventAsync(orderEvent);

            // 3. Lưu xuống Database
            await _repository.SaveChangesAsync();
            _logger.LogWarning("⚠️ [Saga] Đã hủy đơn hàng {OrderId} theo đúng kịch bản Saga.", message.OrderId);
        }
    }
}