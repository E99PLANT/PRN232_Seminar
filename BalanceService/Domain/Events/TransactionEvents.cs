namespace BalanceService.Domain.Events
{
    public record TransactionApproved(Guid Id, decimal Amount, DateTime ApprovedAt);
}
