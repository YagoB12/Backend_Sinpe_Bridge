using Backend_Bridge.DTO;

namespace Backend_Bridge.Services
{
    public class PaymentValidationService
    {
        // CAMBIO: Ahora usamos tu nueva entidad Payment en lugar de una lista de strings
        private static readonly List<Payment> _mockPayments = new List<Payment>();

        // Simulamos la tabla de Fraudes (Tarea #156)
        private static readonly List<FraudAttempt> _mockFraudAttempts = new List<FraudAttempt>();

        // Tarea #155: Implementar validación de referencia duplicada
        public (bool IsValid, string Message) ValidateReference(string reference, decimal amount, string senderNumber)
        {
            // Tarea #157: Lógica de rechazo automático
            // Buscamos si ya existe algún pago con esa misma referencia
            bool isDuplicate = _mockPayments.Any(p => p.Reference == reference);

            if (isDuplicate)
            {
                // Disparamos la alerta y guardamos el fraude
                HandleFraudAttempt(reference, "Referencia Duplicada");
                return (false, "Pago rechazado: La referencia ya fue procesada anteriormente.");
            }

            // Si pasa la validación, guardamos el nuevo pago en nuestra tabla falsa
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

        private void HandleFraudAttempt(string reference, string fraudType)
        {
            // Guardamos el intento en nuestra tabla falsa
            var fraud = new FraudAttempt
            {
                Id = _mockFraudAttempts.Count + 1,
                FraudType = fraudType,
                Reference = reference,
                AttemptDate = DateTime.Now
            };
            _mockFraudAttempts.Add(fraud);

            // Tarea #158: Implementar notificación al administrador
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
