using Backend_Bridge.Data;
using Backend_Bridge.Models;
using System.Globalization;

namespace Backend_Bridge.Services
{
    public class PaymentValidationService
    {
        private readonly ApplicationDbContext _context;

        public PaymentValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // RF-03 -> Validar tiempo
        public (bool IsValid, string Message) ValidateTimeReference(string reference, Order order)
        {
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
                order.Status = "SUSPECTED";
                _context.SaveChanges();

                return (false, "La referencia no contiene una fecha válida.");
            }

            TimeSpan difference = DateTime.Now - paymentDate;

            if (difference.TotalMinutes > 15)
            {
                order.Status = "SUSPECTED";
                _context.SaveChanges();

                return (false, $"Pago sospechoso. Han pasado {(int)difference.TotalMinutes} minutos.");
            }

            return (true, "Tiempo válido.");
        }

        // RF-04 -> validar duplicado
        public (bool IsValid, string Message) ValidateReference(string reference)
        {
            bool isDuplicate = _context.Payments.Any(p => p.Reference == reference);

            if (isDuplicate)
            {
                HandleFraudAttempt(reference, "Referencia duplicada");
                return (false, "La referencia ya fue utilizada.");
            }

            return (true, "Referencia válida.");
        }

        // RF-05 -> VALIDACIÓN PRINCIPAL
        public (bool IsValid, string Message) ValidateAmount(decimal amount, string payerName, string reference, string customerPhone)
        {
            //  BUSCAR LA ORDEN MÁS RECIENTE DEL CLIENTE
            var order = _context.Orders
                .Where(o =>
                    o.CustomerName == payerName &&
                    o.Status == "PENDING")
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (order == null)
                return (false, "No existe una orden pendiente para este cliente.");

            // validar teléfono
            if (NormalizePhone(order.Phone) != NormalizePhone(customerPhone))
            {
                order.Status = "SUSPECTED";
                HandleFraudAttempt(reference, "Teléfono del cliente no coincide");
                _context.SaveChanges();

                return (false, "El número de origen del pago no coincide con el número registrado en la orden.");
            }

            // validar tiempo
            var timeValidation = ValidateTimeReference(reference, order);
            if (!timeValidation.IsValid)
                return timeValidation;

            // validar monto
            if (order.Amount != amount)
            {
                HandleFraudAttempt(reference, "Monto incorrecto");
                return (false, "El monto no coincide con la orden.");
            }

            if (order.Status != "PENDING")
            {
                return (false, "La orden ya fue procesada.");
            }

            //  TODO OK → MARCAR COMO PAGADO
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

                transaction.Commit();

                return (true, "Pago confirmado correctamente.");
            }
            catch
            {
                transaction.Rollback();
                return (false, "Ocurrió un error al registrar el pago.");
            }
        }

        private void HandleFraudAttempt(string reference, string fraudType)
        {
            var fraud = new FraudAttempt
            {
                Reference = reference,
                FraudType = fraudType,
                AttemptDate = DateTime.Now
            };

            _context.FraudAttempts.Add(fraud);
            _context.SaveChanges();
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