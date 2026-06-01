using Backend_Bridge.DTO;

namespace Backend_Bridge.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendTransactionEmailAsync(EmailNotificationDto request);
    }
}
