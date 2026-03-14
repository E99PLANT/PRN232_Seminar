using Marten;
using Marten.Events.Projections;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Events;

namespace TransactionService.Application.Projectors
{
    public class TransactionToRabbitMqProjection : EventProjection
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionToRabbitMqProjection(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Đổi tên từ Handle thành Project để Marten 7.x tự nhận diện
        public async Task Project(TransactionApproved @event, IDocumentOperations operations)
        {
            bool success = false;
            int retryCount = 0;

            while (!success)
            {
                using var scope = _serviceProvider.CreateScope();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                try
                {
                    await eventBus.PublishAsync(@event);
                    Console.WriteLine($"[Success] Đã gửi Event {@event.Id} sang RabbitMQ.");
                    success = true; // Thoát vòng lặp khi gửi thành công
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"[Thử lại lần {retryCount}] RabbitMQ chưa sẵn sàng. Đợi 5 giây...");

                    // Đợi 5 giây rồi thử lại ngay lập tức trong luồng này
                    await Task.Delay(5000);

                    // Lưu ý: Nếu muốn dừng hẳn để xem Dead Letter, bạn có thể break sau 10 lần thử
                    if (retryCount > 20) throw;
                }
            }
        }
    }
}
