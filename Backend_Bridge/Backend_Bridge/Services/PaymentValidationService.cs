using Backend_Bridge.Data;
using Backend_Bridge.Models; // Asegúrate de usar el namespace correcto donde están Payment y FraudAttempt


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

        // guardado de intentos de fraude
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