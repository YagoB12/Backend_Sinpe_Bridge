using Backend_Bridge.DTO;
using Backend_Bridge.Services;
using Backend_Bridge.Data;
using Backend_Bridge.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("sms")]
public class SmsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly SmsService _smsService;
    private readonly PaymentValidationService _paymentValidationService;

    public SmsController(ApplicationDbContext context, SmsService smsService, PaymentValidationService paymentValidationService)
    {
        _smsService = smsService;
        _paymentValidationService = paymentValidationService;
        _context = context;
    }

    [HttpPost]
    public IActionResult ReceiveSms([FromBody] SmsReceive request)
    {
        if (request == null || string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("SMS inválido");

        //Validación de remitente
        if (!_smsService.IsValidSender(request.Sender))
            return BadRequest("Remitente no válido");

        //Filtro de mensajes no válidos (spam)
        if (!_smsService.IsValidSinpeMessage(request.Message))
            return BadRequest("Mensaje no corresponde a SINPE");
        }

        Console.WriteLine($"SMS válido recibido: {request.Message}");

        // prueba para SmsLog sin base de datos
        var newLog = new SmsLog
        {
            SenderNumber = request.Sender,
            MessageBody = request.Message,
            ReceivedAt = DateTime.Now,
            IsProcessed = false,
            IsValidOrigin = true
        };

        //GUARDAR EN BD REAL
        _context.SmsLogs.Add(newLog);
        _context.SaveChanges();

        return Ok(new
        {
            message = "SMS guardado en base de datos",
            logId = newLog.Id
        });
    }

    [HttpGet("logs")]
    public IActionResult GetLogs()
    {
        var logs = _context.SmsLogs.ToList();
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