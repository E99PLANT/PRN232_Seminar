namespace InventoryService.Application.DTOs
{
    public class OrderCreatedEventDto
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
