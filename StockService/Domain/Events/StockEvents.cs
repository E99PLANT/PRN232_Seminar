namespace StockService.Domain.Events;

// Nhập kho
public record StockAdded(Guid ItemId, int Quantity, DateTime AddedAt);

// Trừ kho
public record StockSubtracted(Guid ItemId, int Quantity, DateTime SubtractedAt);

// Hết hàng
public record StockExhausted(Guid ItemId, DateTime OccurredAt);