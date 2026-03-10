using MassTransit;
using NotificationHub.Api.Consumers;
using NotificationHub.Api.Hubs;
using NotificationHub.Api.Endpoints;
using NotificationHub.Api.Services;
using NotificationHub.Application.Extensions;
using NotificationHub.Application.Interfaces;
using NotificationHub.Infrastructure.Extensions;
using NotificationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// OpenAPI
builder.Services.AddOpenApi();

// SignalR
builder.Services.AddSignalR();

// CORS (for thin client development)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IPushNotificationSender, SignalRPushNotificationSender>();

// MassTransit / RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationStatusUpdateConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbit = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbit["Host"], rabbit["VirtualHost"], h =>
        {
            h.Username(rabbit["Username"]!);
            h.Password(rabbit["Password"]!);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Auto-migrate in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseSerilogRequestLogging();

// Notification endpoints
app.MapNotificationEndpoints();

// SignalR hub
app.MapHub<PushNotificationHub>("/hubs/notifications");

await app.RunAsync();

// Make the implicit Program class accessible for WebApplicationFactory<Program>
public partial class Program;
