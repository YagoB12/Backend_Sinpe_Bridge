using Backend_Bridge.Data;
using Backend_Bridge.DTO;
using Backend_Bridge.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Bridge.Controllers
{
    [ApiController]
    [Route("risk")]
    public class RiskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RiskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: /risk/payment/{id}
        // =========================
        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> GetPaymentRisk(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Risk)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return NotFound("Payment no encontrado.");

            var result = new PaymentRiskDto
            {
                PaymentId = payment.Id,
                Reference = payment.Reference,
                Amount = payment.Amount,
                RiskLevel = payment.Risk?.Level ?? "UNKNOWN",
                RiskDescription = payment.Risk?.Description ?? "Sin riesgo calculado"
            };

            return Ok(result);
        }

        // =========================
        // GET: /risk/payments
        // =========================
        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPaymentsRisk()
        {
            var payments = await _context.Payments
                .Include(p => p.Risk)
                .Select(p => new PaymentRiskDto
                {
                    PaymentId = p.Id,
                    Reference = p.Reference,
                    Amount = p.Amount,
                    RiskLevel = p.Risk != null ? p.Risk.Level : "UNKNOWN",
                    RiskDescription = p.Risk != null ? p.Risk.Description : "Sin riesgo"
                })
                .ToListAsync();

            return Ok(payments);
        }
    }
}