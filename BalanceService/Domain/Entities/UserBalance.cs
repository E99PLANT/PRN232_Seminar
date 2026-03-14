namespace BalanceService.Domain.Entities
{
    public class UserBalance
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
