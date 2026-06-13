using Backend_Bridge.Data;
using Backend_Bridge.Models;

namespace Backend_Bridge.Services.Payments
{
    public class FraudAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogService _auditLogService;

        public FraudAuditService(ApplicationDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public void Register(
            string reference,
            decimal amount,
            string fraudType,
            string auditAction,
            string auditDescription,
            int? orderId = null)
        {
            _context.FraudAttempts.Add(new FraudAttempt
            {
                Reference = reference,
                Amount = amount,
                FraudType = fraudType,
                AttemptDate = DateTime.Now
            });

            _auditLogService.Register(
                auditAction,
                auditDescription,
                reference,
                orderId
            );
        }
    }
}
