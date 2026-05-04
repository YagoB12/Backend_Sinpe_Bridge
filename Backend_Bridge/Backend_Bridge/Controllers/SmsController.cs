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

    public SmsController(SmsService smsService)
    {
        _smsService = smsService;
    }

    [HttpPost]
    public IActionResult ReceiveSms([FromBody] SmsReceive request)
    {
        if (request == null || string.IsNullOrEmpty(request.Message))
            return BadRequest("SMS inválido");

        //Validación de remitente
        if (!_smsService.IsValidSender(request.Sender))
        {
            return BadRequest("Remitente no válido (no es SINPE)");
        }

        //Filtro de mensajes no válidos (spam)
        if (!_smsService.IsValidSinpeMessage(request.Message))
        {
            return BadRequest("Mensaje no corresponde a SINPE");
        }

        Console.WriteLine($"SMS válido recibido: {request.Message}");

        var newLog = new SmsLog
        {
            Id = _mockDatabase.Count + 1,
            SenderNumber = request.Sender,
            MessageBody = request.Message,
            ReceivedAt = DateTime.Now,
            IsProcessed = false,
            IsValidOrigin = true // Ya pasó la validación de arriba
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
}