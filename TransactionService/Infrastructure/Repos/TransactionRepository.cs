using Marten;
using TransactionService.Domain.Interfaces;
//using Wolverine;

namespace TransactionService.Infrastructure.Repos
{
    public class TransactionRepository(IDocumentSession session) : ITransactionRepository
    {
        private readonly IDocumentSession _session = session;
        //private readonly IMessageBus _bus = bus;

        public void Create(Guid id, object @event)
        {
            // Khởi tạo một Stream sự kiện mới
            _session.Events.StartStream(id, @event);
        }

        public async Task SaveChangesAsync() => await _session.SaveChangesAsync();
    }
}
