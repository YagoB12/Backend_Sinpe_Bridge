using Backend_Bridge.Data;
using Backend_Bridge.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Bridge.Controllers
{
    [ApiController]
    [Route("manual-review")]
    public class ManualReviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ManualReviewController(ApplicationDbContext context)
        {
            _context = context;
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
        public IActionResult Approve(int id)
        {
            var review = _context.ManualReviewTransactions
                .FirstOrDefault(x => x.Id == id);

            if (review == null)
                return NotFound("Transacción no encontrada.");

            // VALIDAR DOBLE PROCESO
            if (review.ActionType == "APPROVED" ||
                review.ActionType == "REJECTED")
            {
                return BadRequest("La transacción ya fue procesada.");
            }

            review.ActionType = "APPROVED";

            // ACTUALIZAR ORDEN
            if (review.OrderId != null)
            {
                var order = _context.Orders
                    .FirstOrDefault(o => o.Id == review.OrderId);

                if (order != null)
                {
                    order.Status = "PAID";
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Transacción aprobada correctamente."
            });
        }

        // =========================================
        // REJECT
        // =========================================
        [HttpPost("{id}/reject")]
        public IActionResult Reject(int id)
        {
            var review = _context.ManualReviewTransactions
                .FirstOrDefault(x => x.Id == id);

            if (review == null)
                return NotFound("Transacción no encontrada.");

            // VALIDAR DOBLE PROCESO
            if (review.ActionType == "APPROVED" ||
                review.ActionType == "REJECTED")
            {
                return BadRequest("La transacción ya fue procesada.");
            }

            review.ActionType = "REJECTED";

            // ACTUALIZAR ORDEN
            if (review.OrderId != null)
            {
                var order = _context.Orders
                    .FirstOrDefault(o => o.Id == review.OrderId);

                if (order != null)
                {
                    order.Status = "REJECTED";
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Transacción rechazada correctamente."
            });
        }
    }
}