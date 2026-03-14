namespace BalanceService.Application.DTOs
{
    public class BalanceRequest
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
