namespace OrderService.Application.DTOs
{
    public class CreateOrderRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}