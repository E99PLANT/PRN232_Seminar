using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserBehaviorService.Application.Interfaces;
using UserBehaviorService.Contracts;

namespace UserBehaviorService.Application.Consumers
{
    public class UserBehaviorConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;

        public UserBehaviorConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:Host"] ?? "localhost",
                UserName = _configuration["RabbitMq:Username"] ?? "guest",
                Password = _configuration["RabbitMq:Password"] ?? "guest",
                Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672")
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var authExchange = _configuration["RabbitMq:AuthExchange"] ?? "customer.auth";
            var profileExchange = _configuration["RabbitMq:ProfileExchange"] ?? "customer.profile";
            var queue = _configuration["RabbitMq:Queue"] ?? "userbehavior.analytics";

            await _channel.ExchangeDeclareAsync(authExchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.ExchangeDeclareAsync(profileExchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(queue, authExchange, "user.registered", cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue, authExchange, "user.verified", cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue, authExchange, "user.logged_in", cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue, authExchange, "user.locked", cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue, authExchange, "user.unlocked", cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(queue, profileExchange, "userprofile.created", cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue, profileExchange, "userprofile.updated", cancellationToken: cancellationToken);

            await _channel.BasicQosAsync(0, 1, false, cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null) throw new InvalidOperationException("RabbitMQ channel not initialized.");

            var queue = _configuration["RabbitMq:Queue"] ?? "userbehavior.analytics";
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var routingKey = ea.RoutingKey;
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var messageId = ea.BasicProperties?.MessageId;
                    if (string.IsNullOrWhiteSpace(messageId))
                        messageId = Guid.NewGuid().ToString("N");

                    using var scope = _serviceProvider.CreateScope();
                    var analyticsService = scope.ServiceProvider.GetRequiredService<IUserBehaviorAnalyticsService>();

                    switch (routingKey)
                    {
                        case "user.registered":
                            {
                                var msg = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(json)!;
                                await analyticsService.HandleUserRegisteredAsync(messageId, msg.UserId, msg.Email, msg.OccurredOn);
                                break;
                            }
                        case "user.verified":
                            {
                                var msg = JsonSerializer.Deserialize<UserVerifiedIntegrationEvent>(json)!;
                                await analyticsService.HandleUserVerifiedAsync(messageId, msg.UserId, msg.Email, msg.OccurredOn);
                                break;
                            }
                        case "user.logged_in":
                            {
                                var msg = JsonSerializer.Deserialize<UserLoggedInIntegrationEvent>(json)!;
                                await analyticsService.HandleUserLoggedInAsync(
                                    messageId,
                                    msg.UserId,
                                    msg.Email,
                                    msg.OccurredOn,
                                    msg.SessionDurationMinutes);
                                break;
                            }
                        case "user.locked":
                            {
                                var msg = JsonSerializer.Deserialize<UserLockedIntegrationEvent>(json)!;
                                await analyticsService.HandleUserLockedAsync(messageId, msg.UserId, msg.OccurredOn);
                                break;
                            }
                        case "user.unlocked":
                            {
                                var msg = JsonSerializer.Deserialize<UserUnlockedIntegrationEvent>(json)!;
                                await analyticsService.HandleUserUnlockedAsync(messageId, msg.UserId, msg.OccurredOn);
                                break;
                            }
                        case "userprofile.created":
                            {
                                var msg = JsonSerializer.Deserialize<UserProfileCreatedIntegrationEvent>(json)!;
                                await analyticsService.HandleUserProfileCreatedAsync(messageId, msg.UserId, msg.Email, msg.OccurredOn);
                                break;
                            }
                        case "userprofile.updated":
                            {
                                var msg = JsonSerializer.Deserialize<UserProfileUpdatedIntegrationEvent>(json)!;
                                await analyticsService.HandleUserProfileUpdatedAsync(messageId, msg.UserId, msg.OccurredOn);
                                break;
                            }
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UserBehavior consumer error: {ex.Message}");
                    await _channel!.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async void Dispose()
        {
            if (_channel != null) await _channel.DisposeAsync();
            if (_connection != null) await _connection.DisposeAsync();
            base.Dispose();
        }
    }
}
