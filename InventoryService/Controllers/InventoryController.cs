using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/danh-inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryAppService _appService;

        public InventoryController(IInventoryAppService appService)
        {
            _appService = appService;
        }

        [HttpGet("stock/all")]
        public async Task<IActionResult> GetAllStock()
        {
            var stocks = await _appService.GetAllStockAsync();

            // Map Entity sang DTO để giấu bớt các cột không cần thiết (như Id nội bộ)
            var response = stocks.Select(s => new StockResponseDto
            {
                ProductId = s.ProductId,
                ProductName = s.ProductName,
                StockQuantity = s.StockQuantity,
                LastUpdated = s.LastUpdated
            });

            return Ok(response);
        }

        [HttpGet("stock/{productId}")]
        public async Task<IActionResult> GetCurrentStock(Guid productId)
        {
            var stock = await _appService.GetStockAsync(productId);
            if (stock == null) return NotFound(new { Message = "Không tìm thấy sản phẩm trong kho." });

            var response = new StockResponseDto
            {
                ProductId = stock.ProductId,
                ProductName = stock.ProductName,
                StockQuantity = stock.StockQuantity,
                LastUpdated = stock.LastUpdated
            };

            return Ok(response);
        }

        [HttpPost("import-stock")]
        public async Task<IActionResult> ImportStock([FromBody] ImportStockRequestDto request)
        {
            var updatedStock = await _appService.ImportStockAsync(request.ProductId, request.ProductName, request.StockQuantity);

            var response = new StockResponseDto
            {
                ProductId = updatedStock.ProductId,
                ProductName = updatedStock.ProductName,
                StockQuantity = updatedStock.StockQuantity,
                LastUpdated = updatedStock.LastUpdated
            };

            return Ok(new { Message = "Đã nhập kho thành công!", Data = response });
        }

        [HttpDelete("stock/{productId}")]
        public async Task<IActionResult> DeleteStock(Guid productId)
        {
            var result = await _appService.DeleteStockAsync(productId);
            if (!result) return NotFound(new { Message = "Sản phẩm không tồn tại" });
            return Ok(new { Message = "Đã xóa sản phẩm khỏi kho" });
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetRecentEvents([FromQuery] int count = 20)
        {
            var events = await _appService.GetRecentEventsAsync(count);
            // Với Event Store, ta có thể trả thẳng ra vì cấu trúc nó vốn đã là log
            return Ok(events);
        }
    }
}