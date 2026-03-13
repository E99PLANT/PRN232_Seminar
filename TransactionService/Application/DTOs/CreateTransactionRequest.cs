namespace TransactionService.Application.DTOs
{
    public class CreateTransactionRequest
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
