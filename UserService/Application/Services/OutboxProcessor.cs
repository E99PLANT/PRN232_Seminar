using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PRN232_Seminar.Shared.Events;
using UserService.Infrastructure.Data;

namespace UserService.Application.Services;

/// <summary>
/// Background service chạy mỗi 5 giây,
/// đọc OutboxMessages chưa gửi → publish lên RabbitMQ.
/// Nếu RabbitMQ sập → retry lần sau, không mất message.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[OutboxProcessor] Đã khởi động. Polling mỗi {Interval} giây.", _pollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OutboxProcessor] Lỗi khi xử lý outbox messages.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Lấy tối đa 20 messages chưa gửi, cũ nhất trước
        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => !m.IsSent)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (pendingMessages.Count == 0) return;

        _logger.LogInformation("[OutboxProcessor] Tìm thấy {Count} message chưa gửi.", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            try
            {
                // Deserialize và publish đúng loại event
                switch (message.EventType)
                {
                    case nameof(UserCreatedEvent):
                        var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message.EventData);
                        if (userCreatedEvent != null)
                        {
                            await publishEndpoint.Publish(userCreatedEvent, ct);
                        }
                        break;

                    default:
                        _logger.LogWarning("[OutboxProcessor] Không nhận dạng được EventType: {EventType}", message.EventType);
                        break;
                }

                // Đánh dấu đã gửi thành công
                message.IsSent = true;
                message.SentAt = DateTime.UtcNow;
                message.Error = null;

                _logger.LogInformation("[OutboxProcessor] ✅ Đã gửi message {Id} (Type: {Type})", message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                // Gửi thất bại → ghi lỗi, thử lại lần sau
                message.RetryCount++;
                message.Error = ex.Message;

                _logger.LogWarning("[OutboxProcessor] ❌ Gửi thất bại message {Id}, lần thử {Retry}: {Error}",
                    message.Id, message.RetryCount, ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
