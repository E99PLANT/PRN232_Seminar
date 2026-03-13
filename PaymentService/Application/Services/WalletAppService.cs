using MassTransit;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PRN232_Seminar.Shared.Events;
using System.Text.Json;

namespace PaymentService.Application.Services;

public class WalletAppService : IWalletAppService
{
    private readonly IWalletRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public WalletAppService(IWalletRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Tạo Account mới + tự động tạo Wallet rỗng (số dư = 0)
    /// </summary>
    public async Task<WalletDto> CreateAccountAsync(CreateAccountDto dto)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
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
                Username = dto.Username,
                Email = dto.Email,
                Balance = 0,
                Currency = "VND",
                Source = "API (Manual)"
            }),
            Timestamp = DateTime.UtcNow
        });
        await _repository.SaveChangesAsync();

        return MapToWalletDto(created);
    }

    /// <summary>
    /// Xem thông tin Wallet theo AccountId
    /// </summary>
    public async Task<WalletDto?> GetWalletByAccountIdAsync(Guid accountId)
    {
        var wallet = await _repository.GetWalletByAccountIdAsync(accountId);
        if (wallet == null) return null;

        return new WalletDto
        {
            WalletId = wallet.Id,
            AccountId = wallet.AccountId,
            Username = wallet.Account.Username,
            Email = wallet.Account.Email,
            Balance = wallet.Balance,
            Currency = wallet.Currency,
            LastUpdated = wallet.LastUpdated
        };
    }

    /// <summary>
    /// Xử lý giao dịch: Nạp tiền (Deposit) hoặc Rút tiền (Withdraw)
    /// Tự động kiểm tra và đánh dấu hoạt động bất thường
    /// </summary>
    public async Task<TransactionDto> ProcessTransactionAsync(CreateTransactionDto dto)
    {
        var wallet = await _repository.GetWalletByAccountIdAsync(dto.AccountId);
        if (wallet == null)
            throw new Exception($"Không tìm thấy ví cho AccountId: {dto.AccountId}");

        // Validate
        if (dto.Amount <= 0)
            throw new Exception("Số tiền giao dịch phải lớn hơn 0.");

        if (dto.TransactionType == "Withdraw" && wallet.Balance < dto.Amount)
            throw new Exception($"Số dư không đủ. Hiện tại: {wallet.Balance} VND, yêu cầu rút: {dto.Amount} VND.");

        // Tính số dư trước/sau
        decimal balanceBefore = wallet.Balance;
        decimal balanceAfter;

        if (dto.TransactionType == "Deposit")
        {
            balanceAfter = balanceBefore + dto.Amount;
        }
        else if (dto.TransactionType == "Withdraw")
        {
            balanceAfter = balanceBefore - dto.Amount;
        }
        else
        {
            throw new Exception($"Loại giao dịch không hợp lệ: {dto.TransactionType}. Chỉ chấp nhận 'Deposit' hoặc 'Withdraw'.");
        }

        // === PHÁT HIỆN HOẠT ĐỘNG BẤT THƯỜNG ===
        var (isSuspicious, reason) = await DetectSuspiciousActivityAsync(wallet.Id, dto, balanceBefore);

        // Tạo transaction record
        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            TransactionType = dto.TransactionType,
            Amount = dto.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Description = dto.Description,
            Timestamp = DateTime.UtcNow,
            IsSuspicious = isSuspicious,
            SuspiciousReason = reason
        };

        // Cập nhật số dư ví
        wallet.Balance = balanceAfter;
        wallet.LastUpdated = DateTime.UtcNow;

        // Lưu vào DB
        await _repository.AddTransactionAsync(transaction);
        await _repository.UpdateWalletAsync(wallet);

        // Event Sourcing — Ghi event Deposited/Withdrawn
        await _repository.AppendEventAsync(new WalletEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = wallet.Id,
            AggregateType = "Wallet",
            EventType = dto.TransactionType == "Deposit" ? "Deposited" : "Withdrawn",
            EventData = JsonSerializer.Serialize(new
            {
                TransactionId = transaction.Id,
                Amount = dto.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Description = dto.Description
            }),
            Timestamp = DateTime.UtcNow
        });

        // Nếu bất thường → ghi event + publish qua RabbitMQ
        if (isSuspicious)
        {
            // Event Sourcing — Ghi event SuspiciousDetected
            await _repository.AppendEventAsync(new WalletEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = wallet.Id,
                AggregateType = "Wallet",
                EventType = "SuspiciousDetected",
                EventData = JsonSerializer.Serialize(new
                {
                    TransactionId = transaction.Id,
                    Amount = dto.Amount,
                    Reason = reason
                }),
                Timestamp = DateTime.UtcNow
            });

            // RabbitMQ — Publish SuspiciousActivityDetectedEvent → UserService
            await _publishEndpoint.Publish(new SuspiciousActivityDetectedEvent
            {
                WalletId = wallet.Id,
                TransactionId = transaction.Id,
                Username = wallet.Account.Username,
                Amount = dto.Amount,
                Reason = reason!,
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[CẢNH BÁO] Giao dịch bất thường: {reason} | WalletId: {wallet.Id} | Amount: {dto.Amount}");
            Console.WriteLine($"[RabbitMQ] Đã gửi SuspiciousActivityDetectedEvent → UserService");
        }

        await _repository.SaveChangesAsync();

        return MapToTransactionDto(transaction);
    }

    /// <summary>
    /// Lấy lịch sử giao dịch của 1 ví
    /// </summary>
    public async Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid walletId, int count = 20)
    {
        var transactions = await _repository.GetTransactionsByWalletIdAsync(walletId, count);
        return transactions.Select(MapToTransactionDto);
    }

    /// <summary>
    /// Tra cứu tất cả giao dịch bị đánh dấu bất thường
    /// </summary>
    public async Task<IEnumerable<TransactionDto>> GetSuspiciousTransactionsAsync(int count = 20)
    {
        var transactions = await _repository.GetSuspiciousTransactionsAsync(count);
        return transactions.Select(MapToTransactionDto);
    }

    /// <summary>
    /// Event Sourcing — Lấy toàn bộ event history của 1 ví
    /// </summary>
    public async Task<IEnumerable<object>> GetEventsByWalletIdAsync(Guid walletId)
    {
        var events = await _repository.GetEventsByWalletIdAsync(walletId);
        return events.Select(e => new
        {
            e.Id,
            e.AggregateId,
            e.EventType,
            EventData = JsonSerializer.Deserialize<object>(e.EventData),
            e.Timestamp
        });
    }

    // =====================================================================
    // THUẬT TOÁN PHÁT HIỆN HOẠT ĐỘNG BẤT THƯỜNG
    // =====================================================================

    private async Task<(bool IsSuspicious, string? Reason)> DetectSuspiciousActivityAsync(
        Guid walletId, CreateTransactionDto dto, decimal currentBalance)
    {
        var reasons = new List<string>();

        // --- Tiêu chí 1: Giao dịch lớn bất thường ---
        var last30Days = await _repository.GetTransactionsInTimeRangeAsync(
            walletId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        var recentTransactions = last30Days.ToList();
        if (recentTransactions.Any())
        {
            var avgAmount = recentTransactions.Average(t => t.Amount);
            if (avgAmount > 0 && dto.Amount > avgAmount * 5)
            {
                reasons.Add($"Số tiền ({dto.Amount:N0}) gấp {dto.Amount / avgAmount:F1}x trung bình 30 ngày ({avgAmount:N0})");
            }
        }

        // --- Tiêu chí 2: Tần suất giao dịch cao ---
        var lastHour = await _repository.GetTransactionsInTimeRangeAsync(
            walletId, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        var hourlyCount = lastHour.Count();
        if (hourlyCount >= 10)
        {
            reasons.Add($"Tần suất cao: {hourlyCount + 1} giao dịch trong 1 giờ (ngưỡng: 10)");
        }

        // --- Tiêu chí 3: Rút gần hết ví ---
        if (dto.TransactionType == "Withdraw" && currentBalance > 0)
        {
            var withdrawPercentage = dto.Amount / currentBalance * 100;
            if (withdrawPercentage > 90)
            {
                reasons.Add($"Rút {withdrawPercentage:F1}% số dư (> 90% ngưỡng cảnh báo)");
            }
        }

        if (reasons.Any())
        {
            return (true, string.Join(" | ", reasons));
        }

        return (false, null);
    }

    // =====================================================================
    // MAPPING HELPERS
    // =====================================================================

    private static WalletDto MapToWalletDto(Account account)
    {
        return new WalletDto
        {
            WalletId = account.Wallet.Id,
            AccountId = account.Id,
            Username = account.Username,
            Email = account.Email,
            Balance = account.Wallet.Balance,
            Currency = account.Wallet.Currency,
            LastUpdated = account.Wallet.LastUpdated
        };
    }

    private static TransactionDto MapToTransactionDto(WalletTransaction t)
    {
        return new TransactionDto
        {
            Id = t.Id,
            WalletId = t.WalletId,
            TransactionType = t.TransactionType,
            Amount = t.Amount,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            Timestamp = t.Timestamp,
            IsSuspicious = t.IsSuspicious,
            SuspiciousReason = t.SuspiciousReason
        };
    }
}
