using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

public interface IWalletRepository
{
    // === Account ===
    Task<Account?> GetAccountByIdAsync(Guid accountId);
    Task<Account?> GetAccountByUsernameAsync(string username);
    Task<Account> CreateAccountAsync(Account account);

    // === Wallet ===
    Task<Wallet?> GetWalletByAccountIdAsync(Guid accountId);
    Task UpdateWalletAsync(Wallet wallet);

    // === Transactions ===
    Task<WalletTransaction> AddTransactionAsync(WalletTransaction transaction);
    Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int count = 20);

    // === Phát hiện bất thường ===
    // Lấy các giao dịch trong khoảng thời gian (để tính trung bình, đếm tần suất)
    Task<IEnumerable<WalletTransaction>> GetTransactionsInTimeRangeAsync(Guid walletId, DateTime from, DateTime to);

    // Lấy tất cả giao dịch bị đánh dấu bất thường
    Task<IEnumerable<WalletTransaction>> GetSuspiciousTransactionsAsync(int count = 20);

    // === Event Sourcing ===
    Task AppendEventAsync(WalletEvent walletEvent);
    Task<IEnumerable<WalletEvent>> GetEventsByWalletIdAsync(Guid walletId);

    // Lưu thay đổi
    Task SaveChangesAsync();
}
