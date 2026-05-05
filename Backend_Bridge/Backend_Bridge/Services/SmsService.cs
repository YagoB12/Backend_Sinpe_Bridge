using Backend_Bridge.Data;
using Backend_Bridge.Models;

namespace Backend_Bridge.Services
{
    public class SmsService
    {
        private readonly ApplicationDbContext _context;

        public SmsService(ApplicationDbContext context)
        {
            _context = context;
        }

        private readonly List<string> SinpeSenders = new()
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

        public SmsLog SaveSms(string sender, string message)
        {
            var newLog = new SmsLog
            {
                SenderNumber = sender,
                MessageBody = message,
                ReceivedAt = DateTime.Now,
                IsProcessed = false,
                IsValidOrigin = true
            };

            _context.SmsLogs.Add(newLog);
            _context.SaveChanges();

            return newLog;
        }
        public List<SmsLog> GetLogs()
        {
            return _context.SmsLogs.ToList();
        }
    }
}