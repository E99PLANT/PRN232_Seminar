namespace UserProfileService.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string routingKey, object payload);
    }
}
