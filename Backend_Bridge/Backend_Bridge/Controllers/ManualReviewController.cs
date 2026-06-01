using Backend_Bridge.Data;
using Backend_Bridge.DTO;
using Backend_Bridge.Models;
using Backend_Bridge.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Bridge.Controllers
{
    [ApiController]
    [Route("manual-review")]
    public class ManualReviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public ManualReviewController(ApplicationDbContext context, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
        }

        // =========================================
        // GET ALL
        // =========================================
        [HttpGet]
        public IActionResult GetAll()
        {
            var reviews = _context.ManualReviewTransactions
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(reviews);
        }

        // =========================================
        // GET BY ID
        // =========================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var review = _context.ManualReviewTransactions
                .FirstOrDefault(x => x.Id == id);

            if (review == null)
                return NotFound("Transacción no encontrada.");

            return Ok(review);
        }

        // =========================================
        // APPROVE
        // =========================================
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id) 
        {
            var review = _context.ManualReviewTransactions.FirstOrDefault(x => x.Id == id);
            if (review == null) {
                return NotFound("Transacción no encontrada.");
            }

            if (review.ActionType == "APPROVED" || review.ActionType == "REJECTED") {
                return BadRequest("La transacción ya fue procesada.");
            }

            review.ActionType = "APPROVED";

            decimal orderAmount = 0;

            if (review.OrderId != null)
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == review.OrderId);
                if (order != null)
                {
                    order.Status = "PAID";
                    orderAmount = order.Amount;
                }
            }

            _context.SaveChanges();

            // =========================================
            // ENVÍO DE NOTIFICACIÓN DINÁMICO (APROBADO)
            // =========================================
            var adminEmail = _config["SmtpSettings:AdminEmail"];

            if (!string.IsNullOrEmpty(adminEmail))
            {
                try
                {
                    var emailDto = new EmailNotificationDto
                    {
                        RecipientEmail = adminEmail,
                        Amount = orderAmount,
                        Reference = $"Rev. Manual #{review.Id}",
                        Status = "PAID"
                    };

                    await _emailService.SendTransactionEmailAsync(emailDto);

                    _context.EmailNotificationLogs.Add(new EmailNotificationLog
                    {
                        OrderId = review.OrderId ?? 0,
                        RecipientEmail = adminEmail,
                        Subject = "Aviso de Transacción: PAID",
                        Status = "Exitoso",
                        SentAt = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    _context.EmailNotificationLogs.Add(new EmailNotificationLog
                    {
                        OrderId = review.OrderId ?? 0,
                        RecipientEmail = adminEmail,
                        Subject = "Aviso de Transacción: PAID",
                        Status = "Fallido",
                        ErrorMessage = ex.Message,
                        SentAt = DateTime.Now
                    });
                }

                _context.SaveChanges();
            }

            return Ok(new {
                message = "Transacción aprobada correctamente." 
            });
        }

        // =========================================
        // REJECT
        // =========================================
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var review = _context.ManualReviewTransactions.FirstOrDefault(x => x.Id == id);
            if (review == null) return NotFound("Transacción no encontrada.");

            if (review.ActionType == "APPROVED" || review.ActionType == "REJECTED")
                return BadRequest("La transacción ya fue procesada.");

            review.ActionType = "REJECTED";

            decimal orderAmount = 0;

            if (review.OrderId != null)
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == review.OrderId);
                if (order != null)
                {
                    order.Status = "REJECTED";
                    orderAmount = order.Amount;
                }
            }

            _context.SaveChanges();

            // =========================================
            // ENVÍO DE NOTIFICACIÓN DINÁMICO (RECHAZO)
            // =========================================
            var adminEmail = _config["SmtpSettings:AdminEmail"];

            if (!string.IsNullOrEmpty(adminEmail))
            {
                try
                {
                    var emailDto = new EmailNotificationDto
                    {
                        RecipientEmail = adminEmail,
                        Amount = orderAmount,
                        Reference = $"Rev. Manual #{review.Id}",
                        Status = "REJECTED"
                    };

                    await _emailService.SendTransactionEmailAsync(emailDto);

                    _context.EmailNotificationLogs.Add(new EmailNotificationLog
                    {
                        OrderId = review.OrderId ?? 0,
                        RecipientEmail = adminEmail,
                        Subject = "Aviso de Transacción: REJECTED",
                        Status = "Exitoso",
                        SentAt = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    _context.EmailNotificationLogs.Add(new EmailNotificationLog
                    {
                        OrderId = review.OrderId ?? 0,
                        RecipientEmail = adminEmail,
                        Subject = "Aviso de Transacción: REJECTED",
                        Status = "Fallido",
                        ErrorMessage = ex.Message,
                        SentAt = DateTime.Now
                    });
                }

                _context.SaveChanges();
            }

            return Ok(new { 
                message = "Transacción rechazada correctamente y notificación enviada." 
            });
        }
    }
}