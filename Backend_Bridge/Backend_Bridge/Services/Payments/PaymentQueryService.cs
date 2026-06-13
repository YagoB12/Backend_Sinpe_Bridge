using Backend_Bridge.Data;
using Backend_Bridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Bridge.Services.Payments
{
    public class PaymentQueryService
    {
        private readonly ApplicationDbContext _context;

        public PaymentQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<FraudAttempt> GetFraudLogs()
        {
            return _context.FraudAttempts.ToList();
        }

        public IEnumerable<Payment> GetPayments()
        {
            return _context.Payments.ToList();
        }

        public object GetPaymentsDetails()
        {
            return _context.Payments
                .Include(p => p.Order)
                .Select(p => new
                {
                    p.Id,
                    p.Reference,
                    p.Amount,
                    p.PaymentDate,
                    p.Status,
                    p.VerificationResult,
                    p.SenderNumber,
                    CustomerName = p.Order != null ? p.Order.CustomerName : "Sin orden asociada",
                    CustomerPhone = p.Order != null ? p.Order.Phone : p.SenderNumber
                })
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }
    }
}
