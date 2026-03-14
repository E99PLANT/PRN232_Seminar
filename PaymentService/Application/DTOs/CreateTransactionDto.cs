namespace PaymentService.Application.DTOs;

public class CreateTransactionDto
{
    // ID của Account thực hiện giao dịch
    public Guid AccountId { get; set; }

    // Loại giao dịch: "Deposit" (nạp) hoặc "Withdraw" (rút)
    public string TransactionType { get; set; } = string.Empty;

    // Số tiền giao dịch
    public decimal Amount { get; set; }

    // Mô tả giao dịch (tùy chọn)
    public string? Description { get; set; }
}
