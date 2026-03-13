using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Controllers
{
    [ApiController]
    // Route này sẽ khớp với cấu hình "/order-service/{everything}" trong Ocelot API Gateway
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderAppService _appService;

        public OrderController(IOrderAppService appService)
        {
            _appService = appService;
        }

        // 1. API Đặt hàng (Kích hoạt Saga)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            // Bắn event lên RabbitMQ và nhận lại OrderId
            var orderId = await _appService.CreateOrderAsync(request);

            // Trả về HTTP 202 (Accepted) - Báo cho FE biết là yêu cầu đã được nhận và đang xử lý ngầm
            return Accepted(new
            {
                Message = "Đơn hàng đang được xử lý bất đồng bộ qua hệ thống.",
                OrderId = orderId
            });
        }

        // 2. API Lấy thông tin 1 đơn hàng (FE dùng để check xem trạng thái từ Pending đã sang Success/Failed chưa)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _appService.GetOrderAsync(id);
            if (order == null) return NotFound(new { Message = "Không tìm thấy đơn hàng." });

            return Ok(order);
        }

        // 3. API Lấy toàn bộ đơn hàng (Cho màn hình danh sách)
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _appService.GetAllOrdersAsync();
            return Ok(orders);
        }
    }
}