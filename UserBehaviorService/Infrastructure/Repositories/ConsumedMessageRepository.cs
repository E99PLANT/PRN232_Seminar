using Microsoft.EntityFrameworkCore;
using UserBehaviorService.Application.Interfaces;
using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Infrastructure.Repositories
{
    public class ConsumedMessageRepository : IConsumedMessageRepository
    {
        private readonly AppDbContext _context;

        public ConsumedMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string messageId)
        {
            return await _context.ConsumedMessages.AnyAsync(x => x.MessageId == messageId);
        }

        public async Task AddAsync(string messageId, string eventType)
        {
            _context.ConsumedMessages.Add(new ConsumedMessage
            {
                MessageId = messageId,
                EventType = eventType
            });

            await _context.SaveChangesAsync();
        }
    }
}
