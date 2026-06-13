using Backend_Bridge.Constants;
using Backend_Bridge.Data;
using Backend_Bridge.Models;

namespace Backend_Bridge.Services.Payments
{
    public class PaymentRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogService _auditLogService;

        public PaymentRecordService(ApplicationDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public void RegisterAdvancedPayment(decimal amount, string reference, string customerPhone)
        {
            _context.Payments.Add(new Payment
            {
                Reference = reference,
                Amount = amount,
                PaymentDate = DateTime.Now,
                SenderNumber = customerPhone,
                OrderId = null,
                Status = PaymentStatuses.PendingAssociation,
                VerificationResult = "Pago adelantado pendiente de asociar"
            });

            _auditLogService.Register(
                "PAGO_ADELANTADO_REGISTRADO",
                "Se recibió un pago antes de que existiera una orden. Queda pendiente de asociación.",
                reference,
                null
            );

            _context.SaveChanges();
        }

        public void RegisterRejectedDuplicateReference(string reference)
        {
            _context.Payments.Add(new Payment
            {
                Reference = reference,
                Amount = 0,
                PaymentDate = DateTime.Now,
                SenderNumber = "Desconocido",
                OrderId = null,
                Status = PaymentStatuses.Rejected,
                VerificationResult = "Referencia duplicada"
            });

            _context.SaveChanges();
        }

        public void RegisterRejectedPayment(Order order, decimal amount, string reference, string customerPhone, Risk risk, List<string> errors)
        {
            _context.Payments.Add(new Payment
            {
                Reference = reference,
                Amount = amount,
                PaymentDate = DateTime.Now,
                SenderNumber = customerPhone,
                OrderId = order.Id,
                Status = PaymentStatuses.Rejected,
                VerificationResult = string.Join(" | ", errors),
                Risk = risk
            });

            _context.SaveChanges();
        }

        public void RegisterApprovedPayment(Order order, decimal amount, string reference, string customerPhone)
        {
            _context.Payments.Add(new Payment
            {
                Reference = reference,
                Amount = amount,
                PaymentDate = DateTime.Now,
                SenderNumber = customerPhone,
                OrderId = order.Id,
                Status = PaymentStatuses.Approved,
                VerificationResult = "Pago exitoso"
            });

            _context.SaveChanges();
        }
    }
}
