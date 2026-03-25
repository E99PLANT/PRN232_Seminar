using System.Text;
using System.Text.Json;
using AuthService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AuthService.Infrastructure.Messaging.Contracts;

namespace AuthService.Infrastructure.Messaging
{
    public class AuthRawEventsRpcConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthRawEventsRpcConsumer> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public AuthRawEventsRpcConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<AuthRawEventsRpcConsumer> logger)
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
                queue: "auth.raw-events.request",
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
                    var request = JsonSerializer.Deserialize<RawEventsRequestMessage>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                        return;

                    using var scope = _scopeFactory.CreateScope();
                    var eventStore = scope.ServiceProvider.GetRequiredService<IEventStoreService>();

                    var events = await eventStore.GetEventsAsync(request.UserId);

                    var response = new RawEventsResponseMessage
                    {
                        UserId = request.UserId,
                        CorrelationId = correlationId ?? string.Empty,
                        Events = events
                            .OrderBy(x => x.OccurredOn)
                            .Select(x => new EventItemMessage
                            {
                                EventType = x.GetType().Name,
                                OccurredOn = x.OccurredOn,
                                PayloadJson = JsonSerializer.Serialize(x)
                            })
                            .ToList()
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
                    _logger.LogError(ex, "Auth raw events RPC failed");
                }

                await Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(
                queue: "auth.raw-events.request",
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
