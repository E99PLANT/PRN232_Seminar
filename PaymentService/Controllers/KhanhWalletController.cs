using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/khanh-wallet")]
public class KhanhWalletController : ControllerBase
{
    private readonly IWalletAppService _appService;

    public KhanhWalletController(IWalletAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Tạo Account mới + Wallet (số dư = 0)
    /// </summary>
    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        try
        {
            var result = await _appService.CreateAccountAsync(dto);
            return CreatedAtAction(nameof(GetWallet), new { accountId = result.AccountId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Xem thông tin Wallet theo AccountId
    /// </summary>
    [HttpGet("accounts/{accountId}")]
    public async Task<IActionResult> GetWallet(Guid accountId)
    {
        var result = await _appService.GetWalletByAccountIdAsync(accountId);
        if (result == null)
            return NotFound(new { Message = "Không tìm thấy ví cho account này." });

        return Ok(result);
    }

    /// <summary>
    /// Tìm Wallet theo Username (dùng sau khi tạo user qua RabbitMQ)
    /// </summary>
    [HttpGet("accounts/by-username/{username}")]
    public async Task<IActionResult> GetWalletByUsername(string username)
    {
        var result = await _appService.GetWalletByUsernameAsync(username);
        if (result == null)
            return NotFound(new { Message = $"Không tìm thấy ví cho username: {username}. Có thể RabbitMQ chưa xử lý xong." });

        return Ok(result);
    }

    /// <summary>
    /// Tìm Wallet theo Email
    /// </summary>
    [HttpGet("accounts/by-email/{email}")]
    public async Task<IActionResult> GetWalletByEmail(string email)
    {
        var result = await _appService.GetWalletByEmailAsync(email);
        if (result == null)
            return NotFound(new { Message = $"Không tìm thấy ví cho email: {email}. Có thể user chưa tồn tại hoặc ví chưa được tạo." });

        return Ok(result);
    }

    /// <summary>
    /// Nạp tiền vào ví (Deposit)
    /// </summary>
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] CreateTransactionDto dto)
    {
        try
        {
            dto.TransactionType = "Deposit";
            var result = await _appService.ProcessTransactionAsync(dto);
            return Ok(new
            {
                Message = "Nạp tiền thành công!",
                Transaction = result,
                Warning = result.IsSuspicious ? $"⚠️ CẢNH BÁO: {result.SuspiciousReason}" : null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Rút tiền từ ví (Withdraw)
    /// </summary>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] CreateTransactionDto dto)
    {
        try
        {
            dto.TransactionType = "Withdraw";
            var result = await _appService.ProcessTransactionAsync(dto);
            return Ok(new
            {
                Message = "Rút tiền thành công!",
                Transaction = result,
                Warning = result.IsSuspicious ? $"⚠️ CẢNH BÁO: {result.SuspiciousReason}" : null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Xem lịch sử giao dịch của 1 ví
    /// </summary>
    [HttpGet("transactions/{walletId}")]
    public async Task<IActionResult> GetTransactionHistory(Guid walletId, [FromQuery] int count = 20)
    {
        var result = await _appService.GetTransactionHistoryAsync(walletId, count);
        return Ok(result);
    }

    /// <summary>
    /// TRA CỨU HOẠT ĐỘNG BẤT THƯỜNG
    /// Endpoint chính cho yêu cầu: "Phương pháp tra cứu hoạt động bất thường đáng tin cậy"
    /// </summary>
    [HttpGet("suspicious")]
    public async Task<IActionResult> GetSuspiciousTransactions([FromQuery] int count = 20)
    {
        var result = await _appService.GetSuspiciousTransactionsAsync(count);
        return Ok(new
        {
            Message = "Danh sách giao dịch bị đánh dấu bất thường",
            TotalFound = result.Count(),
            Criteria = new
            {
                Rule1 = "Số tiền giao dịch > 5x trung bình 30 ngày",
                Rule2 = "Hơn 10 giao dịch trong 1 giờ",
                Rule3 = "Rút > 90% số dư ví"
            },
            Transactions = result
        });
    }

    /// <summary>
    /// EVENT SOURCING — Xem toàn bộ lịch sử event của 1 ví
    /// Replay lại mọi thay đổi trạng thái: WalletCreated → Deposited → Withdrawn → SuspiciousDetected
    /// </summary>
    [HttpGet("events/{walletId}")]
    public async Task<IActionResult> GetWalletEvents(Guid walletId)
    {
        var events = await _appService.GetEventsByWalletIdAsync(walletId);
        return Ok(new
        {
            Message = "Event Sourcing — Lịch sử sự kiện của ví",
            WalletId = walletId,
            TotalEvents = events.Count(),
            Events = events
        });
    }

    /// <summary>
    /// VERIFY INTEGRITY — Kiểm tra hash chain, phát hiện event bị giả mạo
    /// </summary>
    [HttpGet("events/{walletId}/verify")]
    public async Task<IActionResult> VerifyEventIntegrity(Guid walletId)
    {
        var result = await _appService.VerifyEventIntegrityAsync(walletId);
        return Ok(result);
    }

    /// <summary>
    /// REPLAY & SELF-HEALING — Tái dựng balance từ events, phát hiện hack, tự sửa
    /// </summary>
    [HttpPost("replay/{walletId}")]
    public async Task<IActionResult> ReplayAndHeal(Guid walletId)
    {
        try
        {
            var result = await _appService.ReplayAndHealAsync(walletId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
