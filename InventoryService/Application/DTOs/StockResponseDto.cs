namespace InventoryService.Application.DTOs
{
    public class StockResponseDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}