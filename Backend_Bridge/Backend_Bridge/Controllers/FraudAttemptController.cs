using Backend_Bridge.Data;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Bridge.Controllers
{
    [ApiController]
    [Route("fraud-attempts")]
    public class FraudAttemptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FraudAttemptController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tareas #84 y #86: Listar intentos de fraude con filtros por tipo, referencia y fecha
        [HttpGet]
        public IActionResult GetFraudAttempts([FromQuery] string? type, [FromQuery] string? reference, [FromQuery] string? date)
        {
            var query = _context.FraudAttempts.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(f => f.FraudType == type);

            if (!string.IsNullOrEmpty(reference))
                query = query.Where(f => f.Reference == reference);

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                query = query.Where(f => f.AttemptDate.Date == parsedDate.Date);

            var result = query.OrderByDescending(f => f.AttemptDate).ToList();

            return Ok(result);
        }

        // Tarea #85: Consultar detalle de un intento de fraude
        [HttpGet("{id}")]
        public IActionResult GetFraudAttemptById(int id)
        {
            var fraud = _context.FraudAttempts.FirstOrDefault(f => f.Id == id);

            if (fraud == null)
                return NotFound("Intento de fraude no encontrado.");

            return Ok(fraud);
        }
    }
}