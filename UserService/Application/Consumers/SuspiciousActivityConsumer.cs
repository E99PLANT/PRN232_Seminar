using MassTransit;
using PRN232_Seminar.Shared.Events;
using Microsoft.Extensions.Logging;

namespace UserService.Application.Consumers;

/// <summary>
/// Lắng nghe SuspiciousActivityDetectedEvent từ PaymentService
/// Ghi log cảnh báo khi phát hiện giao dịch bất thường
/// </summary>
public class SuspiciousActivityConsumer : IConsumer<SuspiciousActivityDetectedEvent>
{
    private readonly ILogger<SuspiciousActivityConsumer> _logger;

    public SuspiciousActivityConsumer(ILogger<SuspiciousActivityConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SuspiciousActivityDetectedEvent> context)
    {
        var message = context.Message;

        _logger.LogWarning(
            "🚨 [RabbitMQ] CẢNH BÁO GIAO DỊCH BẤT THƯỜNG từ PaymentService!\n" +
            "   👤 User: {Username}\n" +
            "   💰 Số tiền: {Amount:N0} VND\n" +
            "   📝 Lý do: {Reason}\n" +
            "   🔑 WalletId: {WalletId}\n" +
            "   ⏰ Thời gian: {Timestamp}",
            message.Username,
            message.Amount,
            message.Reason,
            message.WalletId,
            message.Timestamp);

        // TODO: Có thể mở rộng thêm — ví dụ: gửi email cảnh báo, khóa tài khoản, v.v.
        await Task.CompletedTask;
    }
}
