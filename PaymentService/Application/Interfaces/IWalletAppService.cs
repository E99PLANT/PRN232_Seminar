using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces;

public interface IWalletAppService
{
    // Tạo Account + Wallet
    Task<WalletDto> CreateAccountAsync(CreateAccountDto dto);

    // Xem thông tin Wallet
    Task<WalletDto?> GetWalletByAccountIdAsync(Guid accountId);

    // Thực hiện giao dịch (Nạp/Rút tiền) + tự động kiểm tra bất thường
    Task<TransactionDto> ProcessTransactionAsync(CreateTransactionDto dto);

    // Lịch sử giao dịch
    Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid walletId, int count = 20);

    // Tra cứu hoạt động bất thường
    Task<IEnumerable<TransactionDto>> GetSuspiciousTransactionsAsync(int count = 20);

    // Event Sourcing — Xem toàn bộ event history
    Task<IEnumerable<object>> GetEventsByWalletIdAsync(Guid walletId);
}
