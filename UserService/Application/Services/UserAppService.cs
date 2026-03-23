using System.Text.Json;
using MassTransit;
using PRN232_Seminar.Shared.Events;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Application.Services;

public class UserAppService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly UserDbContext _dbContext;

    public UserAppService(IUserRepository userRepository, IPublishEndpoint publishEndpoint, UserDbContext dbContext)
    {
        _userRepository = userRepository;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user == null ? null : MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var user = new User
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            PasswordHash = passwordHash,
            FullName = createUserDto.FullName,
            PhoneNumber = createUserDto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // ============================================================
        // OUTBOX PATTERN: Lưu user + event trong CÙNG 1 transaction
        // → RabbitMQ sập cũng không mất event
        // ============================================================
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Lưu user vào DB
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // 2. Lưu event vào bảng OutboxMessages (thay vì gửi RabbitMQ trực tiếp)
            var outboxMessage = new OutboxMessage
            {
                EventType = nameof(UserCreatedEvent),
                EventData = JsonSerializer.Serialize(new UserCreatedEvent
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Timestamp = DateTime.UtcNow
                }),
                CreatedAt = DateTime.UtcNow,
                IsSent = false
            };

            _dbContext.OutboxMessages.Add(outboxMessage);
            await _dbContext.SaveChangesAsync();

            // 3. Commit cả 2 cùng lúc — đảm bảo user + event luôn đồng bộ
            await transaction.CommitAsync();

            Console.WriteLine($"[Outbox] Đã lưu UserCreatedEvent vào OutboxMessages cho user: {user.Username}");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return MapToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(updateUserDto.Email))
            user.Email = updateUserDto.Email;
        
        if (!string.IsNullOrEmpty(updateUserDto.FullName))
            user.FullName = updateUserDto.FullName;
        
        if (updateUserDto.PhoneNumber != null)
            user.PhoneNumber = updateUserDto.PhoneNumber;
        
        if (updateUserDto.IsActive.HasValue)
            user.IsActive = updateUserDto.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _userRepository.ExistsAsync(id))
            return false;

        await _userRepository.DeleteAsync(id);
        return true;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}

