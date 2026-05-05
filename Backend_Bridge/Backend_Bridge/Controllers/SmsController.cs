using Backend_Bridge.DTO;
using Backend_Bridge.Services;
using Backend_Bridge.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("sms")]
public class SmsController : ControllerBase
{
    private readonly SmsService _smsService;
    private readonly ISmsParserService _smsParserService;

    public SmsController(SmsService smsService, ISmsParserService smsParserService)
    {
        _smsService = smsService;
        _smsParserService = smsParserService;
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

        var parsedSms = _smsParserService.Parse(request.Message);

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
}