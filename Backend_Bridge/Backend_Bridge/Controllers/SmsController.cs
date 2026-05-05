using Backend_Bridge.Models;
using Backend_Bridge.Services;
using Backend_Bridge.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Bridge.Controllers
{
    [ApiController]
    [Route("sms")]
    public class SmsController : ControllerBase
    {
        private readonly SmsService _smsService;
        private readonly ISmsParserService _smsParserService;
        private readonly PaymentValidationService _paymentValidationService;

        public SmsController(
            SmsService smsService,
            ISmsParserService smsParserService,
            PaymentValidationService paymentValidationService)
        {
            _smsService = smsService;
            _smsParserService = smsParserService;
            _paymentValidationService = paymentValidationService;
        }

        [HttpPost]
        public IActionResult ReceiveSms([FromBody] SmsReceive request)
        {
            if (request == null)
                return BadRequest("La solicitud no puede estar vacía.");

            if (string.IsNullOrWhiteSpace(request.Sender))
                return BadRequest("El remitente es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("El mensaje SMS es obligatorio.");

            if (!_smsService.IsValidSender(request.Sender))
                return BadRequest("Remitente no válido.");

            if (!_smsService.IsValidSinpeMessage(request.Message))
                return BadRequest("Mensaje no corresponde a SINPE.");

            var newLog = _smsService.SaveSms(request.Sender, request.Message);

            // datos reales del mensaje
            var parsedSms = _smsParserService.Parse(request.Message);

            // --- INICIO DE TU TAREA RF-04 ---
            // Usamos los datos REALES extraídos por tu compañero
            // OJO: Asumo que "parsedSms" tiene propiedades llamadas "Reference" y "Amount". 
            // Si tu compañero les puso otro nombre (ej. "Referencia", "Monto"), solo ajusta los nombres aquí.
            var validationResult = _paymentValidationService.ValidateReference(parsedSms.Reference, parsedSms.Amount, request.Sender);

            if (!validationResult.IsValid)
            {
                // Si es duplicada, se rechaza inmediatamente (Tarea #157)
                return BadRequest(validationResult.Message);
            }
            // --- FIN DE TU TAREA RF-04 ---

            return Ok(new
            {
                message = "SMS recibido, validado, guardado y analizado correctamente.",
                logId = newLog.Id,
                sender = newLog.SenderNumber,
                receivedAt = newLog.ReceivedAt,
                parsedData = parsedSms
            });
        }

        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            var logs = _smsService.GetLogs();
            return Ok(logs);
        }

        [HttpGet("frauds")]
        public IActionResult GetFrauds()
        {
            return Ok(_paymentValidationService.GetFraudLogs());
        }

        [HttpGet("payments")]
        public IActionResult GetPayments()
        {
            return Ok(_paymentValidationService.GetPayments());
        }
    }
}