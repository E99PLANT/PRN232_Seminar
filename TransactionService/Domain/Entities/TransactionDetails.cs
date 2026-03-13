using TransactionService.Domain.Events;

namespace TransactionService.Domain.Entities
{
    public class TransactionDetails
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Phương thức này giúp bảng tự động cập nhật ngay khi Event được lưu
        public void Apply(TransactionCreated @event)
        {
            Id = @event.Id;
            Amount = @event.Amount;
            Description = @event.Description;
            CreatedAt = @event.CreatedAt;
            Status = "Pending";
        }

        public void Apply(TransactionApproved @event) => Status = "Completed";
        public void Apply(TransactionFailed @event) => Status = "Failed";
    }
}
