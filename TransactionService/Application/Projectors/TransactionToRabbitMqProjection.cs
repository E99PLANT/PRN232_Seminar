using Marten;
using Marten.Events.Projections;
using Polly;
using Polly.Fallback;
using Polly.Retry;
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

        public TransactionToRabbitMqProjection()
        {

        }

        // Đổi tên từ Handle thành Project để Marten 7.x tự nhận diện
        public async Task Project(TransactionApproved @event, IDocumentOperations operations)
        {
            // 1. Cấu hình Fallback: Chạy khi tất cả các lần Retry đều thất bại
            var fallbackOptions = new FallbackStrategyOptions<bool>
            {
                ShouldHandle = new PredicateBuilder<bool>().Handle<Exception>(),
                OnFallback = args =>
                {
                    // ĐÂY LÀ NƠI LOG KHI THẤT BẠI HOÀN TOÀN
                    Console.WriteLine($"[FALLBACK] Đã thử 10 lần nhưng không gửi được Event {@event.Id}.");
                    Console.WriteLine($"[REASON] {args.Outcome.Exception?.Message}");

                    // Bạn có thể đánh dấu vào DB là "Gửi lỗi" ở đây
                    return default;
                }
            };

            // 2. Cấu hình Retry: Thử lại 20 lần
            var retryOptions = new RetryStrategyOptions<bool>
            {
                ShouldHandle = new PredicateBuilder<bool>().Handle<Exception>(),
                MaxRetryAttempts = 10,
                Delay = TimeSpan.FromSeconds(5), // Đợi 5s mỗi lần
                OnRetry = args =>
                {
                    Console.WriteLine($"[RETRY] Lần {args.AttemptNumber + 1}: RabbitMQ chưa sẵn sàng. Đang thử lại...");
                    return default;
                }
            };

            // 3. Kết hợp thành Pipeline: Fallback bọc ngoài Retry
            var pipeline = new ResiliencePipelineBuilder<bool>()
                .AddFallback(fallbackOptions)
                .AddRetry(retryOptions)
                .Build();

            // 4. Thực thi
            await pipeline.ExecuteAsync(async token =>
            {
                using var scope = _serviceProvider.CreateScope();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                await eventBus.PublishAsync(@event);
                Console.WriteLine($"[SUCCESS] Gửi thành công Event {@event.Id}");

                return true;
            });
        }
    }
}
