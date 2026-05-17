using Backend_Bridge.Data;
using Backend_Bridge.Models;

namespace Backend_Bridge.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Register(string action, string description, string? reference = null, int? orderId = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                Description = description,
                Reference = reference,
                OrderId = orderId,
                CreatedAt = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);
            _context.SaveChanges();
        }

        public IEnumerable<AuditLog> GetAll()
        {
            return _context.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }
    }
}