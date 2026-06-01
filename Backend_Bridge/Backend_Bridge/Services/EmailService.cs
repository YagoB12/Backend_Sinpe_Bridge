using Backend_Bridge.DTO;
using Backend_Bridge.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Backend_Bridge.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendTransactionEmailAsync(EmailNotificationDto request)
        {
            // 1. Determinar el color y el texto en español según el estado
            string colorEstado = request.Status == "PAID" ? "#28a745" : request.Status == "REJECTED" ? "#dc3545" : "#ffc107";
            string textoEstado = request.Status == "PAID" ? "Aprobada" : request.Status == "REJECTED" ? "Rechazada" : "Sospechosa";

            // 2. Construir la plantilla HTML (Tareas #139 y #141)
            string cuerpoHtml = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                <div style='background-color: #003366; color: white; padding: 20px; text-align: center;'>
                    <h2>Notificación de Sistema: SINPE Bridge</h2>
                </div>
                <div style='padding: 20px;'>
                    <p>Hola Administrador,</p>
                    <p>Se ha procesado una nueva transacción en el sistema. A continuación los detalles:</p>
                    
                    <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                        <tr style='background-color: #f8f9fa;'>
                            <td style='padding: 10px; border: 1px solid #dee2e6;'><strong>Referencia:</strong></td>
                            <td style='padding: 10px; border: 1px solid #dee2e6;'>{request.Reference}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; border: 1px solid #dee2e6;'><strong>Monto:</strong></td>
                            <td style='padding: 10px; border: 1px solid #dee2e6;'>₡{request.Amount:N2}</td>
                        </tr>
                        <tr style='background-color: #f8f9fa;'>
                            <td style='padding: 10px; border: 1px solid #dee2e6;'><strong>Estado:</strong></td>
                            <td style='padding: 10px; border: 1px solid #dee2e6; color: {colorEstado}; font-weight: bold;'>
                                {textoEstado}
                            </td>
                        </tr>
                    </table>
                    
                    <p style='margin-top: 20px; font-size: 12px; color: #6c757d;'>
                        * Este es un correo generado automáticamente. Por favor no responda a este mensaje.
                    </p>
                </div>
            </div>";

            // 3. Configurar el cliente SMTP leyendo el appsettings.json
            var smtpSettings = _config.GetSection("SmtpSettings");
            var smtpClient = new SmtpClient(smtpSettings["Server"])
            {
                Port = int.Parse(smtpSettings["Port"]!),
                Credentials = new NetworkCredential(smtpSettings["SenderEmail"], smtpSettings["Password"]),
                EnableSsl = true,
            };

            // 4. Preparar el mensaje y enviarlo
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings["SenderEmail"]!, smtpSettings["SenderName"]),
                Subject = $"Aviso de Transacción: {request.Reference} - {textoEstado}",
                Body = cuerpoHtml,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(request.RecipientEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}