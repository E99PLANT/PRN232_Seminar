using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Messaging
{
    public class RabbitMqEventBus(IConnection connection) : IEventBus
    {
        private readonly IConnection _connection = connection;
        private const string ExchangeName = "transaction_events";

        public async Task PublishAsync<T>(T @event) where T : class
        {
            using var channel = await _connection.CreateChannelAsync();

            // Khai báo Exchange (Fanout để nhiều service cùng nhận được)
            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true);

            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            // Gửi tin nhắn bất đồng bộ
            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: string.Empty,
                body: body);
        }
    }
}