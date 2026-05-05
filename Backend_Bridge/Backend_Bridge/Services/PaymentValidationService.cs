using Backend_Bridge.Models;
using Backend_Bridge.Data;

namespace Backend_Bridge.Services
{
    public class PaymentValidationService
    {
        private readonly ApplicationDbContext _context;

        public PaymentValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        //RF-04 → SOLO VALIDAR (NO guardar)
        public (bool IsValid, string Message) ValidateReference(string reference)
        {
            bool isDuplicate = _context.Payments.Any(p => p.Reference == reference);

            if (isDuplicate)
            {
                HandleFraudAttempt(reference, "Referencia Duplicada");
                return (false, "Pago rechazado: La referencia ya fue procesada anteriormente.");
            }

            return (true, "Referencia válida.");
        }

        //RF-05 → validar monto y completar flujo
        public (bool IsValid, string Message) ValidateAmount(decimal amount, string senderNumber, string reference)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Phone == senderNumber && o.Status == "PENDING");

            if (order == null)
                return (false, "No existe una orden pendiente para este cliente.");

            if (order.Amount == amount)
            {
                //marcar orden como pagada
                order.Status = "PAID";

                //guardar payment SOLO aquí
                var newPayment = new Payment
                {
                    Reference = reference,
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    SenderNumber = senderNumber
                };

                _context.Payments.Add(newPayment);
                _context.SaveChanges();

                return (true, "Pago confirmado correctamente.");
            }

            //monto incorrecto
            HandleFraudAttempt(reference, "Monto incorrecto");

            return (false, "El monto no coincide con la orden.");
        }

        private void HandleFraudAttempt(string reference, string fraudType)
        {
            var fraud = new FraudAttempt
            {
                FraudType = fraudType,
                Reference = reference,
                AttemptDate = DateTime.Now
            };

            _context.FraudAttempts.Add(fraud);
            _context.SaveChanges();

            Console.WriteLine("==================================================");
            Console.WriteLine("⚠️ ALERTA DE FRAUDE");
            Console.WriteLine($"Tipo: {fraud.FraudType}");
            Console.WriteLine($"Referencia: {fraud.Reference}");
            Console.WriteLine($"Fecha: {fraud.AttemptDate}");
            Console.WriteLine("==================================================");
        }

        public IEnumerable<FraudAttempt> GetFraudLogs()
        {
            return _context.FraudAttempts.ToList();
        }

        public IEnumerable<Payment> GetPayments()
        {
            return _context.Payments.ToList();
        }
    }
}