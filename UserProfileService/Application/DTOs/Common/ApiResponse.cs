namespace UserProfileService.Application.DTOs.Common
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Msg { get; set; } = default!;
        public T? Data { get; set; }

        public static ApiResponse<T> Success(T? data, string msg = "Success")
            => new() { Status = 200, Msg = msg, Data = data };

        public static ApiResponse<T> Fail(string msg, int status = 400)
            => new() { Status = status, Msg = msg, Data = default };
    }
}
