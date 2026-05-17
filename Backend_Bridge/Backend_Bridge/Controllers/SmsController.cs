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
        private readonly AuditLogService _auditLogService;

        public SmsController(
            SmsService smsService,
            ISmsParserService smsParserService,
            PaymentValidationService paymentValidationService,
            AuditLogService auditLogService)
        {
            _smsService = smsService;
            _smsParserService = smsParserService;
            _paymentValidationService = paymentValidationService;
            _auditLogService = auditLogService;
        }

        [HttpPost]
        public IActionResult ReceiveSms([FromBody] SmsReceive request)
        {
            // =========================
            // VALIDACIONES INICIALES
            // =========================
            if (string.IsNullOrWhiteSpace(request.CustomerPhone))
                return BadRequest("El número de teléfono del cliente es obligatorio.");

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

            // =========================
            // GUARDAR SMS
            // =========================
            var newLog = _smsService.SaveSms(request.Sender, request.Message);

            // =========================
            // PARSEAR SMS (RF-02)
            // =========================
            var parsedSms = _smsParserService.Parse(request.Message);

            // =========================
            // VALIDAR REFERENCIA (RF-04)
            // =========================
            var refValidation = _paymentValidationService.ValidateReference(
                parsedSms.Reference
            );

            if (!refValidation.IsValid)
            {
                return BadRequest(refValidation.Message);
            }

            // =========================
            // VALIDAR MONTO (RF-05)
            // =========================
            var amountValidation = _paymentValidationService.ValidateAmount(
                parsedSms.Amount,
                parsedSms.PayerName,
                parsedSms.Reference,
                request.CustomerPhone
            );

            if (!amountValidation.IsValid)
            {
                return BadRequest(amountValidation.Message);
            }

            // =========================
            // RESPUESTA FINAL
            // =========================
            return Ok(new
            {
                message = "SMS procesado correctamente",
                logId = newLog.Id,
                sender = newLog.SenderNumber,
                receivedAt = newLog.ReceivedAt,
                parsedData = parsedSms
            });
        }

        // =========================
        // DEBUG / CONSULTA
        // =========================

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
        [HttpGet("audit-logs")]
        public IActionResult GetAuditLogs()
        {
            return Ok(_auditLogService.GetAll());
        }
    }
}