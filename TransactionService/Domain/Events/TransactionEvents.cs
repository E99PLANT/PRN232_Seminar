namespace TransactionService.Domain.Events
{
    // Tạo yêu cầu giao dịch
    public record TransactionCreated(Guid Id, decimal Amount, string Description, DateTime CreatedAt);

    // Giao dịch thành công
    public record TransactionApproved(Guid Id, decimal Amount, DateTime ApprovedAt);

    // Giao dịch thất bại (ví dụ: số dư không đủ)
    public record TransactionFailed(Guid Id, string Reason, DateTime FailedAt);
}
