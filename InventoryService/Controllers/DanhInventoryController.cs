using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Controllers
{
    // Cố định Route này chỉ dành riêng cho bài của Danh
    [ApiController]
    [Route("api/danh-inventory")]
    public class DanhInventoryController : ControllerBase
    {
        private readonly IInventoryAppService _appService;

        public DanhInventoryController(IInventoryAppService appService)
        {
            _appService = appService;
        }

        /// <summary>
        /// API dùng để TEST: Giả lập việc nhận sự kiện OrderCreated từ Message Broker (RabbitMQ)
        /// </summary>
        [HttpPost("test-order-created")]
        public async Task<IActionResult> SimulateOrderCreatedEvent([FromBody] OrderCreatedEventDto orderEvent)
        {
            try
            {
                // Gọi sang Service tầng Application để xử lý logic Saga mà ta đã viết ở Bước 2
                await _appService.HandleOrderCreatedAsync(orderEvent);

                return Ok(new
                {
                    Message = "Đã tiếp nhận và xử lý sự kiện OrderCreated thành công. Hãy check DB để xem lịch sử Event Sourcing!",
                    OrderId = orderEvent.OrderId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Có lỗi xảy ra khi xử lý sự kiện", Error = ex.Message });
            }
        }
        [HttpGet("stock/{productId}")]
        public async Task<IActionResult> GetCurrentStock(Guid productId)
        {
            var stock = await _appService.GetStockAsync(productId);
            if (stock == null)
            {
                return NotFound(new { Message = "Không tìm thấy sản phẩm trong kho." });
            }
            return Ok(new
            {
                ProductId = stock.ProductId,
                ProductName = stock.ProductName,
                StockQuantity = stock.StockQuantity,
                LastUpdated = stock.LastUpdated
            });
        }


        [HttpPost("import-stock")]
        public async Task<IActionResult> ImportStock([FromBody] InventoryService.Domain.Entities.Inventory newStock)
        {
            // Inject trực tiếp DbContext vào đây cho lẹ (vì đây chỉ là API test tạm)
            var dbContext = HttpContext.RequestServices.GetRequiredService<InventoryService.Infrastructure.Data.InventoryDbContext>();

            var existing = await dbContext.Inventory.FirstOrDefaultAsync(x => x.ProductId == newStock.ProductId);
            if (existing != null)
            {
                existing.StockQuantity += newStock.StockQuantity;
                existing.LastUpdated = DateTime.UtcNow;
                dbContext.Inventory.Update(existing);
            }
            else
            {
                newStock.Id = Guid.NewGuid();
                newStock.LastUpdated = DateTime.UtcNow;
                await dbContext.Inventory.AddAsync(newStock);
            }

            await dbContext.SaveChangesAsync();
            return Ok(new { Message = "Đã nhập kho thành công!", ProductId = newStock.ProductId, CurrentStock = newStock.StockQuantity });
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetRecentEvents([FromQuery] int count = 10)
        {
            var events = await _appService.GetRecentEventsAsync(count);
            return Ok(events);
        }
    }
}