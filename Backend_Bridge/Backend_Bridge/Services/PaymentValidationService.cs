
using Backend_Bridge.Models;

namespace Backend_Bridge.Services
{
    public class PaymentValidationService
    {
        private readonly ApplicationDbContext _context;

        public PaymentValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // validacion para referencia duplicada
        public (bool IsValid, string Message) ValidateReference(string reference, decimal amount, string senderNumber)
        {
            bool isDuplicate = _context.Payments.Any(p => p.Reference == reference);

            if (isDuplicate)
            {

                HandleFraudAttempt(reference, "Referencia Duplicada");
                return (false, "Pago rechazado: La referencia ya fue procesada anteriormente.");
            }

            // aqui se guarda el nuevo pago
            var newPayment = new Payment
            {
                Reference = reference,
                Amount = amount,
                PaymentDate = DateTime.Now,
                SenderNumber = senderNumber
            };

            _context.Payments.Add(newPayment);
            _context.SaveChanges();

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
                FraudType = fraudType,
                Reference = reference,
                AttemptDate = DateTime.Now
            };

            _context.FraudAttempts.Add(fraud);
            _context.SaveChanges();

            // notificación al administrador
            Console.WriteLine("==================================================");
            Console.WriteLine("⚠️ URGENTE: ALERTA DE FRAUDE ENVIADA AL ADMINISTRADOR");
            Console.WriteLine($"Tipo: {fraud.FraudType}");
            Console.WriteLine($"Referencia: {fraud.Reference}");
            Console.WriteLine($"Fecha: {fraud.AttemptDate}");
            Console.WriteLine("==================================================");
        }

        // Métodos para verlos en Swagger
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