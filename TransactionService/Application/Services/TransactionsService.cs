using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Events;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Services
{
    public class TransactionsService(ITransactionRepository transactionRepo) : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo = transactionRepo;

        public async Task<Guid> CreateTransaction(CreateTransactionRequest request)
        {
            var id = Guid.NewGuid();
            var @event = new TransactionCreated(id, request.Amount, request.Description ?? "", DateTime.UtcNow);

            _transactionRepo.Create(id, @event);
            await _transactionRepo.SaveChangesAsync();

            return id;
        }
    }
}
