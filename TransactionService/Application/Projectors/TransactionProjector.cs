using TransactionService.Domain.Entities;
using TransactionService.Domain.Events;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Application.Projectors
{
    public class TransactionProjector
    {
        // Wolverine sẽ tự động kích hoạt hàm này khi Marten lưu Event thành công
        public async Task Handle(TransactionCreated @event, TransactionDbContext dbContext)
        {
            var entity = new TransactionDetails
            {
                Id = @event.Id,
                Amount = @event.Amount,
                Description = @event.Description,
                CreatedAt = @event.CreatedAt,
                Status = "Pending"
            };

            dbContext.Transactions.Add(entity);
            await dbContext.SaveChangesAsync();
        }
    }
}
