namespace UserBehaviorService.Application.Interfaces
{
    public interface IRabbitMqRpcClient
    {
        Task<TResponse> CallAsync<TRequest, TResponse>(
            string requestQueue,
            TRequest request,
            CancellationToken cancellationToken = default);
    }
}
