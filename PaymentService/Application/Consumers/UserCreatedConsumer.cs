using MassTransit;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PRN232_Seminar.Shared.Events;
using System.Text.Json;

namespace PaymentService.Application.Consumers;

/// <summary>
/// Lắng nghe event UserCreatedEvent từ UserService
/// Khi user mới được tạo → tự động tạo Account + Wallet
/// </summary>
public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly IWalletRepository _repository;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(IWalletRepository repository, ILogger<UserCreatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "💰 [RabbitMQ] Nhận UserCreatedEvent → Tạo ví cho user: {Username} ({Email})",
            message.Username, message.Email);

        // Tạo Account + Wallet
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Username = message.Username,
            Email = message.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Balance = 0,
                Currency = "VND",
                LastUpdated = DateTime.UtcNow
            }
        };

        var created = await _repository.CreateAccountAsync(account);

        // Event Sourcing — Ghi event WalletCreated
        await _repository.AppendEventAsync(new WalletEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = created.Wallet.Id,
            AggregateType = "Wallet",
            EventType = "WalletCreated",
            EventData = JsonSerializer.Serialize(new
            {
                AccountId = created.Id,
                Username = message.Username,
                Email = message.Email,
                Balance = 0,
                Currency = "VND",
                Source = "UserCreatedEvent (RabbitMQ)"
            }),
            Timestamp = DateTime.UtcNow
        });

        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "✅ [RabbitMQ] Đã tạo ví thành công cho user {Username}. WalletId: {WalletId}",
            message.Username, created.Wallet.Id);
    }
}
