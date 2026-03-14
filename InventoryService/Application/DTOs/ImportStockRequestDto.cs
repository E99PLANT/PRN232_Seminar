namespace InventoryService.Application.DTOs
{
    public class ImportStockRequestDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}