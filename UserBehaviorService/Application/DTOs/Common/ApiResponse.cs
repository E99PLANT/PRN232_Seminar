namespace UserBehaviorService.Application.DTOs.Common
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Msg { get; set; } = default!;
        public T? Data { get; set; }
    }
}
