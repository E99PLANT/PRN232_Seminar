namespace TransactionService.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        void Create(Guid id, object @event);
        Task SaveChangesAsync();
    }
}
