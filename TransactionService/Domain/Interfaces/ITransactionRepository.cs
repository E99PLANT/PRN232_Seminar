using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        TransactionDetails? GetById(Guid id);
        List<TransactionDetails> GetAllAsync();
        void Create(Guid id, object @event);
        void Update(TransactionDetails details);
        Task SaveChangesAsync();
    }
}
