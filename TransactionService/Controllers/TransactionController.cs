using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;

namespace TransactionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController(ITransactionService transactionService) : ControllerBase
    {
        private readonly ITransactionService _transactionService = transactionService;

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            var transactionId = await _transactionService.CreateTransaction(request);
            return Ok(new { Id = transactionId });
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveTransaction(Guid id)
        {
            await _transactionService.ApproveTransaction(id);
            return Ok(new { Message = "Transaction Approved. Events broadcasted!" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _transactionService.GetAllTransactions();
            return Ok(list);
        }
    }
}
