using Marten;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Interfaces;
using TransactionService.Infrastructure.Data;
//using Wolverine;

namespace TransactionService.Infrastructure.Repos
{
    public class TransactionRepository(TransactionDbContext context, IDocumentSession session) : ITransactionRepository
    {
        private readonly IDocumentSession _session = session;
        private readonly TransactionDbContext _context = context;

        public void Create(Guid id, object @event)
        {
            // Khởi tạo một Stream sự kiện mới
            _session.Events.StartStream(id, @event);
        }

        public TransactionDetails? GetById(Guid id)
        {
            return _context.Transactions.FirstOrDefault(t => t.Id.Equals(id));
        }

        public void Update(TransactionDetails details)
        {
            _context.Transactions.Update(details);
            _context.SaveChanges();
        }

        public List<TransactionDetails> GetAllAsync()
        {
            return [.. _context.Transactions];
        }

        public async Task SaveChangesAsync() => await _session.SaveChangesAsync();
    }
}
