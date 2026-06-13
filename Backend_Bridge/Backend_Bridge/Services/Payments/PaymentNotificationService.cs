using Backend_Bridge.Data;
using Backend_Bridge.DTO;
using Backend_Bridge.Models;
using Backend_Bridge.Services.Interfaces;

namespace Backend_Bridge.Services.Payments
{
    public class PaymentNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public PaymentNotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
        }

        public async Task SendTransactionNotificationAsync(string status, decimal amount, string reference, int orderId)
        {
            var adminEmail = _config["SmtpSettings:AdminEmail"];
            if (string.IsNullOrEmpty(adminEmail))
                return;

            try
            {
                await _emailService.SendTransactionEmailAsync(new EmailNotificationDto
                {
                    RecipientEmail = adminEmail,
                    Amount = amount,
                    Reference = reference,
                    Status = status
                });

                _context.EmailNotificationLogs.Add(new EmailNotificationLog
                {
                    OrderId = orderId,
                    RecipientEmail = adminEmail,
                    Subject = $"Aviso automático: {status}",
                    Status = "Exitoso",
                    SentAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _context.EmailNotificationLogs.Add(new EmailNotificationLog
                {
                    OrderId = orderId,
                    RecipientEmail = adminEmail,
                    Subject = $"Aviso automático: {status}",
                    Status = "Fallido",
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
