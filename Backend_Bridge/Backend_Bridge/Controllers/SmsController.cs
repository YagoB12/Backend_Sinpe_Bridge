using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("sms")]
public class SmsController : ControllerBase
{
    [HttpPost]
    public IActionResult ReceiveSms([FromBody] SmsReceive request)
    {
        if (request == null || string.IsNullOrEmpty(request.Message))
            return BadRequest("SMS inválido");

        // Aquí vas a procesar el mensaje
        Console.WriteLine($"SMS recibido: {request.Message}");

        return Ok(new { message = "SMS procesado correctamente" });
    }
}