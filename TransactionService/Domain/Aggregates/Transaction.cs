using TransactionService.Domain.Events;

namespace TransactionService.Domain.Aggregates
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public decimal Amount { get; private set; }
        public string Status { get; private set; } = "Pending";

        // Hàm khởi tạo bắt buộc cho Marten
        public Transaction() { }

        // Logic xử lý khi có sự kiện Created
        public void Apply(TransactionCreated @event)
        {
            Id = @event.Id;
            Amount = @event.Amount;
            Status = "Pending";
        }

        // Logic xử lý khi có sự kiện Approved
        public void Apply(TransactionApproved @event)
        {
            Status = "Completed";
        }
    }
}
