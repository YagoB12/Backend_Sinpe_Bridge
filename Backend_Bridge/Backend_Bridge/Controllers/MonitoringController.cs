using Backend_Bridge.Data;
using Backend_Bridge.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Bridge.Controllers
{
    [ApiController]
    public class MonitoringController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MonitoringController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tarea #95 y #96: Endpoint POST para recibir señales de vida del teléfono
        [HttpPost("device-heartbeat")]
        public IActionResult ReceiveHeartbeat()
        {
            var device = _context.DeviceHeartbeats.FirstOrDefault(d => d.DeviceName == "Teléfono Principal POS");

            if (device == null)
            {
                device = new DeviceHeartbeat
                {
                    DeviceName = "Teléfono Principal POS",
                    LastCommunication = DateTime.Now,
                    IsConnected = true
                };
                _context.DeviceHeartbeats.Add(device);
            }
            else
            {
                device.LastCommunication = DateTime.Now;
                device.IsConnected = true;
            }

            _context.SaveChanges();

            return Ok(new { Message = "Señal de vida (Heartbeat) recibida correctamente." });
        }

        // Tarea #101: Endpoint GET para consultar el estado actual
        [HttpGet("device-status")]
        public IActionResult GetDeviceStatus()
        {
            var device = _context.DeviceHeartbeats.FirstOrDefault(d => d.DeviceName == "Teléfono Principal POS");

            if (device == null)
                return NotFound("No hay dispositivos registrados en el sistema.");

            return Ok(device);
        }

        // Tarea #102: Endpoint GET para consultar el historial de fallos
        [HttpGet("monitoring-history")]
        public IActionResult GetMonitoringHistory()
        {
            var history = _context.MonitoringHistories
                .OrderByDescending(m => m.DisconnectedAt)
                .ToList();

            return Ok(history);
        }
    }
}