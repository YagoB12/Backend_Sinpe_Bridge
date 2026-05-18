using Backend_Bridge.Data;
using Backend_Bridge.Hubs;
using Backend_Bridge.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backend_Bridge.Services
{
    // Tarea #97: Tarea programada en segundo plano (BackgroundService)
    public class DeviceMonitoringService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<PaymentNotificationHub> _hubContext;

        public DeviceMonitoringService(IServiceScopeFactory scopeFactory, IHubContext<PaymentNotificationHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Buscamos si el dispositivo está marcado como conectado
                    var device = context.DeviceHeartbeats
                        .FirstOrDefault(d => d.DeviceName == "Teléfono Principal POS" && d.IsConnected);

                    if (device != null)
                    {
                        TimeSpan timeSinceLastHeartbeat = DateTime.Now - device.LastCommunication;

                        // Tarea #97: Verificar si pasaron más de 15 minutos sin comunicación
                        if (timeSinceLastHeartbeat.TotalMinutes > 15)
                        {
                            // Tarea #98: Marcar el dispositivo como desconectado
                            device.IsConnected = false;

                            // Tarea #100: Registrar el fallo en el historial de monitoreo
                            var failureLog = new MonitoringHistory
                            {
                                DisconnectedAt = DateTime.Now,
                                Message = $"El teléfono receptor perdió conexión. Última señal recibida hace {(int)timeSinceLastHeartbeat.TotalMinutes} minutos.",
                                IsResolved = false
                            };

                            context.MonitoringHistories.Add(failureLog);
                            context.SaveChanges();

                            // Tarea #99: Enviar alerta automática en tiempo real al administrador por SignalR
                            await _hubContext.Clients.All.SendAsync("DeviceDisconnectedAlert", new
                            {
                                message = failureLog.Message,
                                disconnectedAt = failureLog.DisconnectedAt
                            });
                        }
                    }
                }

                // Se espera 1 minuto antes de volver a verificar el reloj
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}