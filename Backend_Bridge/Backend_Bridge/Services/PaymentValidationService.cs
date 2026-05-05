using Backend_Bridge.Models;
using Backend_Bridge.Data;

namespace Backend_Bridge.Services
{
    public class PaymentValidationService
    {
        private static readonly List<Payment> _mockPayments = new List<Payment>();
        private static readonly List<FraudAttempt> _mockFraudAttempts = new List<FraudAttempt>();

        // NUEVO: acceso a BD
        private readonly ApplicationDbContext _context;

        //NUEVO: constructor
        public PaymentValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        //RF-04 (SIN CAMBIOS)
        public (bool IsValid, string Message) ValidateReference(string reference, decimal amount, string senderNumber)
        {
            bool isDuplicate = _mockPayments.Any(p => p.Reference == reference);

            if (isDuplicate)
            {
                HandleFraudAttempt(reference, "Referencia Duplicada");
                return (false, "Pago rechazado: La referencia ya fue procesada anteriormente.");
            }

            var newPayment = new Payment
            {
                Id = _mockPayments.Count + 1,
                Reference = reference,
                Amount = amount,
                PaymentDate = DateTime.Now,
                SenderNumber = senderNumber
            };

            _mockPayments.Add(newPayment);

            return (true, "Referencia válida y pago registrado en el sistema.");
        }

        //NUEVO  RF-05 REAL
        public (bool IsValid, string Message) ValidateAmount(decimal amount, string senderNumber)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Phone == senderNumber && o.Status == "PENDING");

            if (order == null)
                return (false, "No existe una orden pendiente para este cliente.");

            if (order.Amount == amount)
            {
                order.Status = "PAID";
                _context.SaveChanges();

                return (true, "Pago confirmado correctamente.");
            }

            // monto incorrecto → fraude
            HandleFraudAttempt("N/A", "Monto incorrecto");

            return (false, "El monto no coincide con la orden.");
        }

        //SIN CAMBIOS
        private void HandleFraudAttempt(string reference, string fraudType)
        {
            var fraud = new FraudAttempt
            {
                Id = _mockFraudAttempts.Count + 1,
                FraudType = fraudType,
                Reference = reference,
                AttemptDate = DateTime.Now
            };

            _mockFraudAttempts.Add(fraud);

            Console.WriteLine("==================================================");
            Console.WriteLine("⚠️ URGENTE: ALERTA DE FRAUDE ENVIADA AL ADMINISTRADOR");
            Console.WriteLine($"Tipo: {fraud.FraudType}");
            Console.WriteLine($"Referencia: {fraud.Reference}");
            Console.WriteLine($"Fecha: {fraud.AttemptDate}");
            Console.WriteLine("==================================================");
        }

        public IEnumerable<FraudAttempt> GetFraudLogs()
        {
            return _mockFraudAttempts;
        }

        public IEnumerable<Payment> GetPayments()
        {
            return _mockPayments;
        }
    }
}