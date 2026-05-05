using Backend_Bridge.DTO;
using Backend_Bridge.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("sms")]
public class SmsController : ControllerBase
{
    // lista de prueba para GET de SmsLog
    private static readonly List<SmsLog> _mockDatabase = new List<SmsLog>();

    private readonly SmsService _smsService;
    private readonly PaymentValidationService _paymentValidationService;

    public SmsController(SmsService smsService, PaymentValidationService paymentValidationService)
    {
        _smsService = smsService;
        _paymentValidationService = paymentValidationService;
    }

    [HttpPost]
    public IActionResult ReceiveSms([FromBody] SmsReceive request)
    {
        if (request == null || string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("SMS inválido");
        }
           
        if (!_smsService.IsValidSender(request.Sender))
        {
            return BadRequest("Remitente no válido (no es SINPE)");
        }

        if (!_smsService.IsValidSinpeMessage(request.Message))
        {
            return BadRequest("Mensaje no corresponde a SINPE");
        }

        // variables de prueba
        string extractedReference = "REF-SIMULADA-12345";
        decimal extractedAmount = 5000;

        var validationResult = _paymentValidationService.ValidateReference(extractedReference, extractedAmount, request.Sender);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Message);
        }

        Console.WriteLine($"SMS válido recibido: {request.Message}");

        // prueba para SmsLog sin base de datos
        var newLog = new SmsLog
        {
            Id = _mockDatabase.Count + 1,
            SenderNumber = request.Sender,
            MessageBody = request.Message,
            ReceivedAt = DateTime.Now,
            IsProcessed = false,
            IsValidOrigin = true
        };

        _mockDatabase.Add(newLog);

        return Ok(new
        {
            message = "SMS válido procesado y guardado correctamente",
            logId = newLog.Id
        });

        //return Ok(new { message = "SMS válido procesado correctamente" });
    }

    [HttpGet("logs")]
    public IActionResult GetLogs()
    {
        return Ok(_mockDatabase);
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