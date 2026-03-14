using TransactionService.Application.DTOs;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransaction(CreateTransactionRequest request);
        Task ApproveTransaction(Guid id);
        List<TransactionDetails> GetAllTransactions();
    }
}
