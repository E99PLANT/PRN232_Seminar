using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using UserBehaviorService.Application.Interfaces;
using RabbitMQ.Client.Events;

namespace UserBehaviorService.Infrastructure.Messaging
{
    public class RabbitMqRpcClient : IRabbitMqRpcClient, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _replyQueueName;
        private readonly AsyncEventingBasicConsumer _consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pending = new();

        public RabbitMqRpcClient(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:Host"] ?? "localhost",
                Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
                UserName = configuration["RabbitMq:Username"] ?? "guest",
                Password = configuration["RabbitMq:Password"] ?? "guest"
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            var declareOk = _channel.QueueDeclareAsync(
                queue: "",
                durable: false,
                exclusive: true,
                autoDelete: true).GetAwaiter().GetResult();

            _replyQueueName = declareOk.QueueName;

            _consumer = new AsyncEventingBasicConsumer(_channel);
            _consumer.ReceivedAsync += async (_, ea) =>
            {
                var correlationId = ea.BasicProperties?.CorrelationId;

                if (!string.IsNullOrWhiteSpace(correlationId) &&
                    _pending.TryRemove(correlationId, out var tcs))
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    tcs.TrySetResult(body);
                }

                await Task.CompletedTask;
            };

            _channel.BasicConsumeAsync(
                queue: _replyQueueName,
                autoAck: true,
                consumer: _consumer).GetAwaiter().GetResult();
        }

        public async Task<TResponse> CallAsync<TRequest, TResponse>(
            string requestQueue,
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var correlationId = Guid.NewGuid().ToString("N");
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            _pending[correlationId] = tcs;

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = _replyQueueName,
                ContentType = "application/json"
            };

            var json = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: requestQueue,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            using var ctr = linkedCts.Token.Register(() =>
            {
                if (_pending.TryRemove(correlationId, out var pending))
                {
                    pending.TrySetCanceled(linkedCts.Token);
                }
            });

            string responseJson;
            try
            {
                responseJson = await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException($"RPC timeout for queue '{requestQueue}'.");
            }

            var result = JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new Exception($"RPC response from queue '{requestQueue}' is null.");

            return result;
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
