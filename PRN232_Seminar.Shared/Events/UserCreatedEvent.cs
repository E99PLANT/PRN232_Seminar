namespace PRN232_Seminar.Shared.Events;

/// <summary>
/// Event phát ra khi UserService tạo user mới
/// PaymentService subscribe để tự động tạo Wallet
/// </summary>
public record UserCreatedEvent
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
