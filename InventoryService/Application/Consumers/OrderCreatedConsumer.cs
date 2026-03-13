using MassTransit;
using PRN232_Seminar.Shared.Events;
using InventoryService.Domain.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InventoryService.Application.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        IInventoryRepository inventoryRepository,
        ILogger<OrderCreatedConsumer> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("📦 [RabbitMQ] Nhận yêu cầu trừ kho cho Đơn hàng: {OrderId}", message.OrderId);

        // 1. Tìm sản phẩm
        var product = await _inventoryRepository.GetInventoryByProductIdAsync(message.ProductId);

        // 2. KỊCH BẢN THẤT BẠI (Hết hàng hoặc không thấy SP)
        if (product == null || product.StockQuantity < message.Quantity)
        {
            var reason = product == null ? "Sản phẩm không tồn tại" : "Không đủ số lượng trong kho";

            // Ghi log Event Sourcing cho kịch bản thất bại
            await _inventoryRepository.AppendEventAsync(new InventoryEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = message.ProductId,
                AggregateType = "Inventory",
                EventType = "StockReservationFailed",
                EventData = JsonSerializer.Serialize(new { OrderId = message.OrderId, Reason = reason }),
                Timestamp = DateTime.UtcNow
            });
            await _inventoryRepository.SaveChangesAsync();

            await context.Publish(new StockReservationFailedEvent
            {
                OrderId = message.OrderId,
                ProductId = message.ProductId,
                Reason = reason
            });
            return;
        }

        // 3. KỊCH BẢN THÀNH CÔNG
        // A. Trừ số lượng trong Entity
        product.StockQuantity -= message.Quantity;
        product.LastUpdated = DateTime.UtcNow;

        // B. Ghi log sự kiện StockReserved (Để Radar FE hiển thị)
        var stockReservedEvent = new InventoryEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = product.ProductId,
            AggregateType = "Inventory",
            EventType = "StockReserved",
            EventData = JsonSerializer.Serialize(new
            {
                OrderId = message.OrderId,
                Quantity = message.Quantity,
                Remaining = product.StockQuantity
            }),
            Timestamp = DateTime.UtcNow
        };

        // C. Đẩy vào Repository
        await _inventoryRepository.UpdateInventoryAsync(product);
        await _inventoryRepository.AppendEventAsync(stockReservedEvent);

        // D. CHỐT HẠ: Lưu tất cả thay đổi xuống Database (CỰC KỲ QUAN TRỌNG)
        await _inventoryRepository.SaveChangesAsync();

        _logger.LogInformation("✅ [Saga Success] Đã trừ kho và lưu DB. Tồn kho mới: {Remaining}", product.StockQuantity);

        // E. Bắn tin báo thành công về cho OrderService
        await context.Publish(new InventoryReservedEvent
        {
            OrderId = message.OrderId,
            ProductId = message.ProductId,
            Quantity = message.Quantity
        });
    }
}