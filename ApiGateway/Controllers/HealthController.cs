using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for API Gateway
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check requested");

        return Ok(new
        {
            Status = "Healthy",
            Service = "API Gateway",
            Timestamp = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }

    /// <summary>
    /// Detailed health check with downstream services status
    /// </summary>
    [HttpGet("detailed")]
    public IActionResult GetDetailed()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "API Gateway",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            DownstreamServices = new[]
            {
                new { Name = "UserService", Url = "http://localhost:5209", Status = "Unknown" },
                new { Name = "OrderService", Url = "http://localhost:5267", Status = "Unknown" },
                new { Name = "InventoryService", Url = "http://localhost:5254", Status = "Unknown" },
                new { Name = "PaymentService", Url = "http://localhost:5253", Status = "Unknown" },
                new { Name = "NotificationService", Url = "http://localhost:5100", Status = "Unknown" },
            new { Name = "BalanceService", Url = "http://localhost:5200", Status = "Unknown" },
            new { Name = "TransactionService", Url = "http://localhost:5300", Status = "Unknown" }
            }
        });
    }
}
