using System.Globalization;
using Backend_Bridge.Constants;
using Backend_Bridge.Models;

namespace Backend_Bridge.Services.Payments
{
    public class PaymentRuleValidator
    {
        private readonly AuditLogService _auditLogService;
        private readonly FraudAuditService _fraudAuditService;

        public PaymentRuleValidator(AuditLogService auditLogService, FraudAuditService fraudAuditService)
        {
            _auditLogService = auditLogService;
            _fraudAuditService = fraudAuditService;
        }

        public List<string> Validate(Order order, decimal amount, string reference, string customerPhone)
        {
            var errors = new List<string>();

            ValidateCustomerPhone(order, customerPhone, reference, amount, errors);
            ValidateTimeReference(reference, order, amount, errors);
            ValidateOrderAmount(order, amount, reference, errors);
            ValidateOrderStatus(order, reference, errors);

            return errors;
        }

        private void ValidateCustomerPhone(
            Order order,
            string customerPhone,
            string reference,
            decimal amount,
            List<string> errors)
        {
            if (NormalizePhone(order.Phone) == NormalizePhone(customerPhone))
                return;

            errors.Add("El número de origen del pago no coincide con el número registrado en la orden.");

            _fraudAuditService.Register(
                reference,
                amount,
                "Teléfono del cliente no coincide",
                "TELEFONO_NO_COINCIDE",
                "El número de origen del pago no coincide con el número registrado en la orden.",
                order.Id
            );
        }

        private void ValidateTimeReference(
            string reference,
            Order order,
            decimal amount,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(reference) || reference.Length < 14)
            {
                errors.Add("La referencia no contiene una fecha válida.");
                _fraudAuditService.Register(reference, amount, "Fecha de referencia inválida", "FECHA_REFERENCIA_INVALIDA", "La referencia no contiene una fecha válida.", order.Id);
                return;
            }

            var datePart = reference.Substring(0, 14);

            var isValidDate = DateTime.TryParseExact(
                datePart,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var paymentDate
            );

            if (!isValidDate)
            {
                errors.Add("La referencia no contiene una fecha válida.");
                _fraudAuditService.Register(reference, amount, "Fecha de referencia inválida", "FECHA_REFERENCIA_INVALIDA", "La referencia no contiene una fecha válida.", order.Id);
                return;
            }

            var paymentDateUtc = DateTime.SpecifyKind(paymentDate, DateTimeKind.Utc);
            var difference = paymentDateUtc - order.CreatedAt;

            if (difference.TotalMinutes > 15 && difference.TotalMinutes < 30)
            {
                var message = $"Pago sospechoso. Han pasado {(int)difference.TotalMinutes} minutos.";
                errors.Add(message);

                _fraudAuditService.Register(reference, amount, "Pago fuera de tiempo", "PAGO_FUERA_DE_TIEMPO", message, order.Id);
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

            _fraudAuditService.Register(
                reference,
                amount,
                "Monto incorrecto",
                "MONTO_INCORRECTO",
                "El monto recibido no coincide con el monto registrado en la orden.",
                order.Id
            );
        }

        private void ValidateOrderStatus(Order order, string reference, List<string> errors)
        {
            if (order.Status == OrderStatuses.Pending)
                return;

            errors.Add("La orden ya fue procesada.");

            _auditLogService.Register(
                "ORDEN_YA_PROCESADA",
                "La orden ya fue procesada previamente.",
                reference,
                order.Id
            );
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }
}
