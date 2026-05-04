using Backend_Bridge.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("sms")]
public class SmsController : ControllerBase
{
    private readonly SmsService _smsService;

    public SmsController()
    {
        _smsService = new SmsService();
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

        return Ok(new { message = "SMS válido procesado correctamente" });
    }
}