using TransactionService.Application.DTOs;

namespace TransactionService.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransaction(CreateTransactionRequest request);
    }
}
