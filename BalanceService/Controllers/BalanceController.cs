using BalanceService.Application.DTOs;
using BalanceService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BalanceService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceController(IBalanceService balanceService) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = balanceService.GetBalance();
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BalanceRequest req)
        {
            await balanceService.CreateBalance(req);
            return Ok("Khởi tạo số dư thành công");
        }


    }
}
