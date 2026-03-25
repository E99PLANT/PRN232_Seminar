using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Events;
using TransactionService.Domain.Interfaces;
using Wolverine;

namespace TransactionService.Application.Services
{
    public class TransactionsService(ITransactionRepository transactionRepo, IMessageBus bus) : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo = transactionRepo;
        private readonly IMessageBus _messageBus = bus;


        public async Task<Guid> CreateTransaction(CreateTransactionRequest request)
        {
            var id = Guid.NewGuid();
            var @event = new TransactionCreated(id, request.Amount, request.Description ?? "", DateTime.UtcNow.AddHours(-5));

            _transactionRepo.Create(id, @event);
            await _transactionRepo.SaveChangesAsync();

            await _messageBus.PublishAsync(@event);

            return id;
        }

        public async Task ApproveTransaction(Guid id)
        {
            var entity = _transactionRepo.GetById(id)
                ?? throw new Exception("Entity not found with id: " + id);

            var @event = new TransactionApproved(id, entity.Amount, DateTime.UtcNow);

            // 1. Lưu vào Event Store của Transaction (Marten)
            var eventId = Guid.NewGuid();
            _transactionRepo.Create(eventId, @event);
            await _transactionRepo.SaveChangesAsync();

            // 1.1 Cập nhập entity
            entity.Status = "Completed";
            _transactionRepo.Update(entity);
        }

        public List<TransactionDetails> GetAllTransactions()
        {
            return _transactionRepo.GetAllAsync();
        }
    }
}
