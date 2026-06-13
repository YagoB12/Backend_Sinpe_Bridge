using Backend_Bridge.Constants;
using Backend_Bridge.Data;
using Backend_Bridge.Models;

namespace Backend_Bridge.Services.Payments
{
    public class OrderLookupService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogService _auditLogService;

        public OrderLookupService(ApplicationDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public Order? FindPendingOrder(string payerName)
        {
            return _context.Orders
                .Where(o =>
                    o.CustomerName == payerName &&
                    o.Status == OrderStatuses.Pending &&
                    DateTime.Now <= o.CreatedAt.AddMinutes(30))
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();
        }

        public (bool IsValid, string Message) ValidateExpiredOrder(string payerName, string reference)
        {
            var expiredOrder = _context.Orders
                .Where(o =>
                    o.CustomerName == payerName &&
                    o.Status == OrderStatuses.Expired)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (expiredOrder == null)
                return (true, "La orden no expirada");

            _auditLogService.Register(
                "PAGO_RECIBIDO_ORDEN_EXPIRADA",
                "Se recibió un pago para una orden expirada. El pago no fue asociado.",
                reference,
                expiredOrder.Id
            );

            return (false, "La orden ya expiró. Debe crear una nueva orden.");
        }

        public void ExpirePendingOrders()
        {
            var expiredOrders = _context.Orders
                .Where(o =>
                    o.Status == OrderStatuses.Pending &&
                    DateTime.Now > o.CreatedAt.AddMinutes(30))
                .ToList();

            foreach (var order in expiredOrders)
            {
                order.Status = OrderStatuses.Expired;

                _auditLogService.Register(
                    "ORDEN_EXPIRADA",
                    "La orden expiró automáticamente después de 30 minutos sin pago.",
                    null,
                    order.Id
                );
            }

            _context.SaveChanges();
        }
    }
}
