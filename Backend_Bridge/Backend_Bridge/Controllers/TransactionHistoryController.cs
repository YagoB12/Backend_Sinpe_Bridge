using Backend_Bridge.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Backend_Bridge.Controllers
{
    [ApiController]
    [Route("transaction-history")]
    public class TransactionHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetHistory([FromQuery] string? status, [FromQuery] string? reference, [FromQuery] string? date)
        {
            var query = _context.Payments.Include(p => p.Order).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(reference))
                query = query.Where(p => p.Reference == reference);

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                query = query.Where(p => p.PaymentDate.Date == parsedDate.Date);

            var result = query.OrderByDescending(p => p.PaymentDate)
                .Select(p => new {
                    p.Id,
                    p.PaymentDate,
                    p.Amount,
                    PayerName = p.Order != null ? p.Order.CustomerName : "Desconocido",
                    p.Reference,
                    p.Status,
                    p.VerificationResult
                }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetHistoryById(int id)
        {
            var transaction = _context.Payments
                .Include(p => p.Order)
                .Select(p => new {
                    p.Id,
                    p.PaymentDate,
                    p.Amount,
                    PayerName = p.Order != null ? p.Order.CustomerName : "Desconocido",
                    p.Reference,
                    p.Status,
                    p.VerificationResult,
                    p.SenderNumber,
                    p.OrderId
                })
                .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
                return NotFound("Transacción no encontrada en el historial.");

            return Ok(transaction);
        }
    }
}