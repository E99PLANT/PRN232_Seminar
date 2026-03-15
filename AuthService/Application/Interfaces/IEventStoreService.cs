using AuthService.Domain.Events;

namespace AuthService.Application.Interfaces
{
    public interface IEventStoreService
    {
        Task AppendAsync(string aggregateId, string aggregateType, DomainEvent @event);
        Task<List<DomainEvent>> GetEventsAsync(string aggregateId);
    }
}
