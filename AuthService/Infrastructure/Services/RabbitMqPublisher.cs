using AuthService.Application.Interfaces;
using AuthService.Contracts;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.Infrastructure.Services
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly IConfiguration _configuration;

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishUserVerifiedAsync(string userId, string email)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:Host"] ?? "localhost",
                UserName = _configuration["RabbitMq:Username"] ?? "guest",
                Password = _configuration["RabbitMq:Password"] ?? "guest",
                Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672")
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            var exchange = _configuration["RabbitMq:Exchange"] ?? "customer.auth";
            var routingKey = "user.verified";

            await channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            var message = new UserVerifiedIntegrationEvent
            {
                UserId = userId,
                Email = email,
                OccurredOn = DateTime.UtcNow
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                body: body);
        }
    }
}
