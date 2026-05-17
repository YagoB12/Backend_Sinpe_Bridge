using Backend_Bridge.Data;
using Backend_Bridge.Models;
using System.Globalization;

namespace Backend_Bridge.Services
{
    public class PaymentValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogService _auditLogService;

        public PaymentValidationService(ApplicationDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public (bool IsValid, string Message) ValidateReference(string reference)
        {
            if (IsReferenceDuplicated(reference))
            {
                RegisterFraudAndAudit(
                    reference,
                    "Referencia duplicada",
                    "REFERENCIA_DUPLICADA",
                    "Se detectó un intento de pago con una referencia ya utilizada."
                );

                _context.SaveChanges();

                return (false, "La referencia ya fue utilizada.");
            }

            return (true, "Referencia válida.");
        }

        public (bool IsValid, string Message) ValidateAmount(
            decimal amount,
            string payerName,
            string reference,
            string customerPhone)
        {
            var errors = new List<string>();

            var order = FindPendingOrder(payerName);

            if (order == null)
                return (false, "No existe una orden pendiente para este cliente.");

            ValidateCustomerPhone(order, customerPhone, reference, errors);
            ValidateTimeReference(reference, order, errors);
            ValidateOrderAmount(order, amount, reference, errors);
            ValidateOrderStatus(order, reference, errors);

            if (errors.Any())
            {
                order.Status = "SUSPECTED";

                _auditLogService.Register(
                    "ORDEN_SUSPENDIDA",
                    "La orden fue suspendida por fallos de validación. Debe crear la orden nuevamente.",
                    reference,
                    order.Id
                );

                _context.SaveChanges();

                var finalMessage =
                    "La orden fue suspendida por errores de validación. Debe crear la orden nuevamente. Errores: "
                    + string.Join(" | ", errors);

                return (false, finalMessage);
            }

            return ConfirmPayment(order, amount, reference, customerPhone);
        }

        private Order? FindPendingOrder(string payerName)
        {
            return _context.Orders
                .Where(o =>
                    o.CustomerName == payerName &&
                    o.Status == "PENDING")
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();
        }

        private void ValidateCustomerPhone(
            Order order,
            string customerPhone,
            string reference,
            List<string> errors)
        {
            if (NormalizePhone(order.Phone) == NormalizePhone(customerPhone))
                return;

            errors.Add("El número de origen del pago no coincide con el número registrado en la orden.");

            RegisterFraudAndAudit(
                reference,
                "Teléfono del cliente no coincide",
                "TELEFONO_NO_COINCIDE",
                "El número de origen del pago no coincide con el número registrado en la orden.",
                order.Id
            );
        }

        private void ValidateTimeReference(
            string reference,
            Order order,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(reference) || reference.Length < 14)
            {
                errors.Add("La referencia no contiene una fecha válida.");

                _auditLogService.Register(
                    "FECHA_REFERENCIA_INVALIDA",
                    "La referencia no contiene una fecha válida.",
                    reference,
                    order.Id
                );

                return;
            }

            string datePart = reference.Substring(0, 14);

            bool isValidDate = DateTime.TryParseExact(
                datePart,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime paymentDate
            );

            if (!isValidDate)
            {
                errors.Add("La referencia no contiene una fecha válida.");

                _auditLogService.Register(
                    "FECHA_REFERENCIA_INVALIDA",
                    "La referencia no contiene una fecha válida.",
                    reference,
                    order.Id
                );

                return;
            }

            TimeSpan difference = DateTime.Now - paymentDate;

            if (difference.TotalMinutes > 15)
            {
                var message = $"Pago sospechoso. Han pasado {(int)difference.TotalMinutes} minutos.";

                errors.Add(message);

                _auditLogService.Register(
                    "PAGO_FUERA_DE_TIEMPO",
                    message,
                    reference,
                    order.Id
                );
            }
        }

        private void ValidateOrderAmount(
            Order order,
            decimal amount,
            string reference,
            List<string> errors)
        {
            if (order.Amount == amount)
                return;

            errors.Add("El monto no coincide con la orden.");

            RegisterFraudAndAudit(
                reference,
                "Monto incorrecto",
                "MONTO_INCORRECTO",
                "El monto recibido no coincide con el monto registrado en la orden.",
                order.Id
            );
        }

        private void ValidateOrderStatus(
            Order order,
            string reference,
            List<string> errors)
        {
            if (order.Status == "PENDING")
                return;

            errors.Add("La orden ya fue procesada.");

            _auditLogService.Register(
                "ORDEN_YA_PROCESADA",
                "La orden ya fue procesada previamente.",
                reference,
                order.Id
            );
        }

        private (bool IsValid, string Message) ConfirmPayment(
            Order order,
            decimal amount,
            string reference,
            string customerPhone)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                order.Status = "PAID";

                var payment = new Payment
                {
                    Reference = reference,
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    SenderNumber = customerPhone,
                    OrderId = order.Id
                };

                _context.Payments.Add(payment);
                _context.SaveChanges();

                _auditLogService.Register(
                    "PAGO_CONFIRMADO",
                    "El pago fue asociado correctamente con la orden y la orden fue marcada como pagada.",
                    reference,
                    order.Id
                );

                transaction.Commit();

                return (true, "Pago confirmado correctamente.");
            }
            catch
            {
                transaction.Rollback();
                return (false, "Ocurrió un error al registrar el pago.");
            }
        }

        private bool IsReferenceDuplicated(string reference)
        {
            return _context.Payments.Any(p => p.Reference == reference);
        }

        private void RegisterFraudAndAudit(
            string reference,
            string fraudType,
            string auditAction,
            string auditDescription,
            int? orderId = null)
        {
            var fraud = new FraudAttempt
            {
                Reference = reference,
                FraudType = fraudType,
                AttemptDate = DateTime.Now
            };

            _context.FraudAttempts.Add(fraud);

            _auditLogService.Register(
                auditAction,
                auditDescription,
                reference,
                orderId
            );
        }

        public IEnumerable<FraudAttempt> GetFraudLogs()
        {
            return _context.FraudAttempts.ToList();
        }

        public IEnumerable<Payment> GetPayments()
        {
            return _context.Payments.ToList();
        }

        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }
}