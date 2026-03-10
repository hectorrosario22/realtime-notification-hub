using NotificationHub.Application.DTOs;
using NotificationHub.Application.Interfaces;

namespace NotificationHub.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        // Push notification (real-time via SignalR)
        group.MapPost("/push", SendPushNotification)
            .WithName("SendPushNotification")
            .WithSummary("Send a real-time push notification via SignalR");

        // Async channels (queued via RabbitMQ)
        group.MapPost("/email", SendEmailNotification)
            .WithName("SendEmailNotification")
            .WithSummary("Enqueue an email notification for async delivery");

        group.MapPost("/sms", SendSmsNotification)
            .WithName("SendSmsNotification")
            .WithSummary("Enqueue an SMS notification for async delivery");

        group.MapPost("/whatsapp", SendWhatsAppNotification)
            .WithName("SendWhatsAppNotification")
            .WithSummary("Enqueue a WhatsApp notification for async delivery");

        // Status queries
        group.MapGet("/{id:guid}", GetNotificationById)
            .WithName("GetNotificationById")
            .WithSummary("Get the status and details of a notification by ID");

        group.MapGet("/failed", GetFailedNotifications)
            .WithName("GetFailedNotifications")
            .WithSummary("Get all notifications that failed after maximum retry attempts");

        return app;
    }

    // --- Handlers ---

    private static async Task<IResult> SendPushNotification(
        SendPushNotificationRequest request,
        INotificationService service,
        CancellationToken ct)
    {
        var response = await service.SendPushAsync(request, ct);
        return Results.Created($"/api/notifications/{response.Id}", response);
    }

    private static IResult SendEmailNotification()
    {
        // TODO: Inject INotificationService, publish to email queue via MassTransit
        return Results.Accepted();
    }

    private static IResult SendSmsNotification()
    {
        // TODO: Inject INotificationService, publish to SMS queue via MassTransit
        return Results.Accepted();
    }

    private static IResult SendWhatsAppNotification()
    {
        // TODO: Inject INotificationService, publish to WhatsApp queue via MassTransit
        return Results.Accepted();
    }

    private static async Task<IResult> GetNotificationById(
        Guid id,
        INotificationService service,
        CancellationToken ct)
    {
        var response = await service.GetByIdAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }

    private static async Task<IResult> GetFailedNotifications(
        INotificationService service,
        CancellationToken ct)
    {
        var response = await service.GetFailedAsync(ct);
        return Results.Ok(response);
    }
}
