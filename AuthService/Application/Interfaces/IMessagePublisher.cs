namespace AuthService.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishUserVerifiedAsync(string userId, string email);
        Task PublishAsync(string routingKey, object payload);
    }
}
