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

    private static IResult SendPushNotification()
    {
        // TODO: Inject INotificationService, send via SignalR, persist to DB
        return Results.Ok();
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

    private static IResult GetNotificationById(Guid id)
    {
        // TODO: Inject INotificationService, query notification by ID from DB
        return Results.Ok();
    }

    private static IResult GetFailedNotifications()
    {
        // TODO: Inject INotificationService, query failed notifications from DB
        return Results.Ok();
    }
}
