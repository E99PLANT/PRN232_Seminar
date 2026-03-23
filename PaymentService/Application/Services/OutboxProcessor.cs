using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Data;
using PRN232_Seminar.Shared.Events;

namespace PaymentService.Application.Services;

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
        _logger.LogInformation("[OutboxProcessor-Payment] Đã khởi động. Polling mỗi {Interval} giây.", _pollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OutboxProcessor-Payment] Lỗi khi xử lý outbox messages.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => !m.IsSent)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (pendingMessages.Count == 0) return;

        _logger.LogInformation("[OutboxProcessor-Payment] Tìm thấy {Count} message chưa gửi.", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            try
            {
                switch (message.EventType)
                {
                    case nameof(SuspiciousActivityDetectedEvent):
                        var suspiciousEvent = JsonSerializer.Deserialize<SuspiciousActivityDetectedEvent>(message.EventData);
                        if (suspiciousEvent != null)
                        {
                            await publishEndpoint.Publish(suspiciousEvent, ct);
                        }
                        break;

                    default:
                        _logger.LogWarning("[OutboxProcessor-Payment] Không nhận dạng được EventType: {EventType}", message.EventType);
                        break;
                }

                message.IsSent = true;
                message.SentAt = DateTime.UtcNow;
                message.Error = null;

                _logger.LogInformation("[OutboxProcessor-Payment] ✅ Đã gửi message {Id} (Type: {Type})", message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                _logger.LogWarning("[OutboxProcessor-Payment] ❌ Gửi thất bại message {Id}, lần thử {Retry}: {Error}",
                    message.Id, message.RetryCount, ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
