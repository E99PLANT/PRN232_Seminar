using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserProfileService.Application.Interfaces;
using UserProfileService.Infrastructure.Messaging.Contracts;

namespace UserProfileService.Infrastructure.Messaging
{
    public class UserProfileReplayRpcConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserProfileReplayRpcConsumer> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public UserProfileReplayRpcConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<UserProfileReplayRpcConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:Host"] ?? "localhost",
                Port = int.TryParse(_configuration["RabbitMq:Port"], out var port) ? port : 5672,
                UserName = _configuration["RabbitMq:Username"] ?? "guest",
                Password = _configuration["RabbitMq:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: "profile.replay.request",
                durable: false,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null) return;

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var replyTo = ea.BasicProperties?.ReplyTo;
                var correlationId = ea.BasicProperties?.CorrelationId;

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var request = JsonSerializer.Deserialize<UserProfileReplayRequestMessage>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                        return;

                    using var scope = _scopeFactory.CreateScope();
                    var userProfileService = scope.ServiceProvider.GetRequiredService<IUserProfileService>();

                    var data = await userProfileService.ReplayAsync(request.UserId);

                    var response = new UserProfileReplayResponseMessage
                    {
                        UserId = data.UserId,
                        CorrelationId = correlationId ?? string.Empty,
                        Email = data.Email,
                        ProfileUpdateCount = data.ProfileUpdateCount,
                        LastProfileUpdatedAt = data.LastProfileUpdatedAt
                    };

                    if (!string.IsNullOrWhiteSpace(replyTo))
                    {
                        var replyProps = new BasicProperties
                        {
                            CorrelationId = correlationId,
                            ContentType = "application/json"
                        };

                        var responseJson = JsonSerializer.Serialize(response);
                        var responseBody = Encoding.UTF8.GetBytes(responseJson);

                        await _channel.BasicPublishAsync(
                            exchange: "",
                            routingKey: replyTo,
                            mandatory: false,
                            basicProperties: replyProps,
                            body: responseBody,
                            cancellationToken: stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Profile replay RPC failed");
                }

                await Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(
                queue: "profile.replay.request",
                autoAck: true,
                consumer: consumer,
                cancellationToken: stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
                await _channel.DisposeAsync();

            if (_connection != null)
                await _connection.DisposeAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}
