namespace Backend_Bridge.Services
{
    public class SmsService
    {
        private readonly List<string> SinpeSenders = new List<string>
        {
            "SINPE",
            "Banco Nacional",
            "BAC",
            "BCR"
        };

        public bool IsValidSender(string sender)
        {
            if (string.IsNullOrEmpty(sender))
                return false;

            return SinpeSenders.Any(s => sender.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsValidSinpeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            return message.Contains("SINPE", StringComparison.OrdinalIgnoreCase)
                && message.Contains("Referencia", StringComparison.OrdinalIgnoreCase)
                && message.Contains("recibido", StringComparison.OrdinalIgnoreCase);
        }
    }
}