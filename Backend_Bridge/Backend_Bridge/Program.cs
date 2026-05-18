using Backend_Bridge.Data;
using Backend_Bridge.Services;
using Backend_Bridge.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Backend_Bridge.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Se agregan los servicios
builder.Services.AddScoped<SmsService>();
builder.Services.AddScoped<PaymentValidationService>();

builder.Services.AddScoped<ISmsParserService, SmsParserService>();

builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<ManualVericationService>();

builder.Services.AddHostedService<DeviceMonitoringService>();

builder.Services.AddSignalR();

//Conexión a base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalRTest", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSignalRTest");

app.UseAuthorization();

app.MapControllers();

app.MapHub<PaymentNotificationHub>("/paymentHub");

app.Run();