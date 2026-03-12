using InventoryService.Application.DTOs;
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

        // Thêm 2 hàm này vào trong class InventoryAppService
        public async Task<InventoryService.Domain.Entities.Inventory?> GetStockAsync(Guid productId)
        {
            return await _repository.GetInventoryByProductIdAsync(productId);
        }

        public async Task<IEnumerable<InventoryService.Domain.Entities.InventoryEvent>> GetRecentEventsAsync(int count = 10)
        {
            return await _repository.GetRecentEventsAsync(count);
        }


        public async Task HandleOrderCreatedAsync(OrderCreatedEventDto orderEvent)
        {
            //1.Lay thong tin ton kho hien tai
            var inventory = await _repository.GetInventoryByProductIdAsync(orderEvent.ProductId);
            //2.Logic kiem tra saga pattern
            if (inventory != null && inventory.StockQuantity >= orderEvent.Quantity)
            {
                //Thanh cong: du hang
                //A. Tru kho tamj thoi
                inventory.StockQuantity -= orderEvent.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;
                await _repository.UpdateInventoryAsync(inventory);

                //B. Ghi lai su kien Thanh cong vao EnventStore
                var successEvent = new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    AggregateId = orderEvent.OrderId,
                    AggregateType = "Order",
                    EventType = "StockReserved",
                    EventData = JsonSerializer.Serialize(new { orderEvent.ProductId, orderEvent.Quantity, Status = "Success" })
                };
                await _repository.AppendEventAsync(successEvent);

                //C. Luu tat ca vao DB

                await _repository.SaveChangesAsync();
                // (Tương lai): Tại đây ta sẽ Publish sự kiện "StockReserved" ra RabbitMQ để OrderService nghe thấy và chốt đơn.
                Console.WriteLine($"[SAGA] Đã giữ hàng thành công cho Order: {orderEvent.OrderId}");
            }
            else
            {
                // THẤT BẠI: Hết hàng hoặc không tìm thấy sản phẩm
                // A. Ghi lại sự kiện Thất Bại vào EventStore (Không trừ kho)
                var failedEvent = new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    AggregateId = orderEvent.OrderId,
                    AggregateType = "Order",
                    EventType = "StockReservationFailed", // Sự kiện: Giữ hàng thất bại
                    EventData = JsonSerializer.Serialize(new { orderEvent.ProductId, Reason = "Out of stock" })
                };
                await _repository.AppendEventAsync(failedEvent);

                // B. Lưu vào DB
                await _repository.SaveChangesAsync();

                // (Tương lai): Tại đây ta sẽ Publish sự kiện "StockReservationFailed" ra RabbitMQ để OrderService nghe thấy và HỦY ĐƠN.
                Console.WriteLine($"[SAGA] Thất bại! Không đủ hàng cho Order: {orderEvent.OrderId}");
            }
        }
    }
}
