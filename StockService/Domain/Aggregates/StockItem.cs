using StockService.Domain.Events;

namespace StockService.Domain.Aggregates
{
    public class StockItem
    {
        public Guid Id { get; set; }
        public int CurrentStock { get; private set; }


        public void Apply(StockAdded e) => CurrentStock += e.Quantity;
        public void Apply(StockSubtracted e) => CurrentStock -= e.Quantity;

    }
}
