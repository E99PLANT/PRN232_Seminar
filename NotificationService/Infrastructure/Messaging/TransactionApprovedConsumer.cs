using System.Text;
using System.Text.Json;
using NotificationService.Application.Handlers;
using NotificationService.Domain.Events;
using NotificationService.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class TransactionApprovedConsumer(IConnection connection, IServiceProvider serviceProvider) : BackgroundService
    {
        private readonly IConnection _connection = connection;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync("transaction_events", ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
            var queueOk = await channel.QueueDeclareAsync(queue: "notification_update_queue", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.QueueBindAsync(queueOk.QueueName, "transaction_events", string.Empty, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<TransactionApproved>(message);

                if (@event != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                    // Handle transaction events
                    var notificationHandler = scope.ServiceProvider.GetRequiredService<INotificationHandler>();
                    await notificationHandler.HandleAsync(@event);
                }
            };

            await channel.BasicConsumeAsync(queueOk.QueueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
