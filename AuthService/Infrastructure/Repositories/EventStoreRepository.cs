using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly AppDbContext _context;

        public EventStoreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EventStoreRecord record)
        {
            await _context.EventStoreRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountByAggregateIdAsync(string aggregateId)
        {
            return await _context.EventStoreRecords.CountAsync(x => x.AggregateId == aggregateId);
        }

        public async Task<List<EventStoreRecord>> GetByAggregateIdAsync(string aggregateId)
        {
            return await _context.EventStoreRecords
                .Where(x => x.AggregateId == aggregateId)
                .OrderBy(x => x.Version)
                .ToListAsync();
        }
    }
}