using AuthService.Application.DTOs.Auth;
using AuthService.Application.Interfaces;
using AuthService.Domain.Enums;
using AuthService.Domain.Events;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailSender _emailSender;
        private readonly IEventStoreService _eventStoreService;
        private readonly IMessagePublisher _messagePublisher;

        public AuthService(
            IAccountRepository accountRepository,
            IEmailSender emailSender,
            IEventStoreService eventStoreService,
            IMessagePublisher messagePublisher)
        {
            _accountRepository = accountRepository;
            _emailSender = emailSender;
            _eventStoreService = eventStoreService;
            _messagePublisher = messagePublisher;
        }

        public async Task RequestRegisterOtpAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLower();

            if (!email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only gmail.com is allowed.");

            var account = await _accountRepository.GetByEmailAsync(email);

            if (account != null && account.Status == AccountStatus.Active)
                throw new Exception("This email already has an account.");

            var otp = new Random().Next(100000, 999999).ToString();

            if (account == null)
            {
                account = new Account
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Status = AccountStatus.Pending,
                    RegisterOtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
                    RegisterOtpExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                    RegisterOtpFailCount = 0
                };

                await _accountRepository.AddAsync(account);

                await _eventStoreService.AppendAsync(account.Id, "Account", new UserRegisteredEvent
                {
                    UserId = account.Id,
                    Email = account.Email
                });
            }
            else
            {
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                account.RegisterOtpHash = BCrypt.Net.BCrypt.HashPassword(otp);
                account.RegisterOtpExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5);
                account.RegisterOtpFailCount = 0;
                account.UpdatedAt = DateTimeOffset.UtcNow;

                await _accountRepository.UpdateAsync(account);
            }

            await _emailSender.SendOtpAsync(account.Email, otp);
        }

        public async Task<object> VerifyRegisterOtpAsync(VerifyOtpRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var account = await _accountRepository.GetByEmailAsync(email)
                ?? throw new Exception("Account not found.");

            if (account.RegisterOtpExpiresAt == null || account.RegisterOtpExpiresAt < DateTimeOffset.UtcNow)
                throw new Exception("OTP expired.");

            if (string.IsNullOrWhiteSpace(account.RegisterOtpHash))
                throw new Exception("OTP not found.");

            if (!BCrypt.Net.BCrypt.Verify(request.Otp, account.RegisterOtpHash))
            {
                account.RegisterOtpFailCount++;
                account.UpdatedAt = DateTimeOffset.UtcNow;
                await _accountRepository.UpdateAsync(account);

                throw new Exception("Invalid OTP.");
            }

            account.Status = AccountStatus.Active;
            account.RegisterOtpVerifiedAt = DateTimeOffset.UtcNow;
            account.RegisterOtpHash = null;
            account.RegisterOtpExpiresAt = null;
            account.RegisterOtpFailCount = 0;
            account.UpdatedAt = DateTimeOffset.UtcNow;

            await _accountRepository.UpdateAsync(account);

            await _eventStoreService.AppendAsync(account.Id, "Account", new UserOtpVerifiedEvent
            {
                UserId = account.Id,
                Email = account.Email
            });

            //await _profileProvisionClient.CreateProfileAsync(account.Id, account.Email);
            await _messagePublisher.PublishUserVerifiedAsync(account.Id, account.Email);

            return new
            {
                accountId = account.Id,
                email = account.Email,
                status = account.Status.ToString(),
                message = "Verify successfully"
            };
        }

        public async Task<object> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var account = await _accountRepository.GetByEmailAsync(email)
                ?? throw new Exception("Account not found.");

            if (account.Status == AccountStatus.Pending)
                throw new Exception("Account not verified.");

            if (account.Status == AccountStatus.Locked)
                throw new Exception("Account is locked.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
                throw new Exception("Invalid password.");

            await _eventStoreService.AppendAsync(account.Id, "Account", new UserLoggedInEvent
            {
                UserId = account.Id,
                Email = account.Email
            });

            return new
            {
                accountId = account.Id,
                email = account.Email,
                status = account.Status.ToString(),
                message = "Login successfully"
            };
        }

        public async Task LockAsync(string userId, string reason)
        {
            var account = await _accountRepository.GetByIdAsync(userId)
                ?? throw new Exception("Account not found.");

            if (account.Status == AccountStatus.Locked)
                throw new Exception("Account is already locked.");

            account.Status = AccountStatus.Locked;
            account.UpdatedAt = DateTimeOffset.UtcNow;

            await _accountRepository.UpdateAsync(account);

            await _eventStoreService.AppendAsync(account.Id, "Account", new UserLockedEvent
            {
                UserId = account.Id,
                Reason = reason
            });
        }

        public async Task UnlockAsync(string userId)
        {
            var account = await _accountRepository.GetByIdAsync(userId)
                ?? throw new Exception("Account not found.");

            if (account.Status == AccountStatus.Active)
                throw new Exception("Account is already active.");

            account.Status = AccountStatus.Active;
            account.UpdatedAt = DateTimeOffset.UtcNow;

            await _accountRepository.UpdateAsync(account);

            await _eventStoreService.AppendAsync(account.Id, "Account", new UserUnlockedEvent
            {
                UserId = account.Id
            });
        }
    }
}
