using Microsoft.AspNetCore.Mvc;
using NotificationHub.Api.DTOs.Requests;
using NotificationHub.Api.DTOs.Responses;
using NotificationHub.Api.Entities;
using NotificationHub.Api.Enums;
using NotificationHub.Api.Interfaces;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(
    INotificationRepository repository,
    INotificationLogRepository logRepository,
    ILogger<NotificationsController> logger)
    : ControllerBase
{
    [HttpPost("email")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendEmailNotification(
        [FromBody] SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending email notification to {RecipientId}", request.RecipientId);

        var notification = Notification.CreateEmail(
            request.RecipientId,
            request.Subject,
            request.HtmlBody);

        var created = await repository.AddAsync(notification, cancellationToken);

        await CreateLogEntryAsync(created.Id, created.Channel, NotificationEventType.Created,
            "Email notification created", cancellationToken);

        return CreatedAtAction(nameof(GetNotificationById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPost("sms")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendSmsNotification(
        [FromBody] SendSmsRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending SMS notification to {RecipientId}", request.RecipientId);

        var notification = Notification.CreateSms(
            request.RecipientId,
            request.Content);

        var created = await repository.AddAsync(notification, cancellationToken);

        await CreateLogEntryAsync(created.Id, created.Channel, NotificationEventType.Created,
            "SMS notification created", cancellationToken);

        return CreatedAtAction(nameof(GetNotificationById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPost("whatsapp")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendWhatsAppNotification(
        [FromBody] SendWhatsAppRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending WhatsApp notification to {RecipientId}", request.RecipientId);

        var notification = Notification.CreateWhatsApp(
            request.RecipientId,
            request.TemplateName,
            request.Parameters);

        var created = await repository.AddAsync(notification, cancellationToken);

        await CreateLogEntryAsync(created.Id, created.Channel, NotificationEventType.Created,
            "WhatsApp notification created", cancellationToken);

        return CreatedAtAction(nameof(GetNotificationById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPost("push")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendPushNotification(
        [FromBody] SendPushRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending push notification to {RecipientId}", request.RecipientId);

        var notification = Notification.CreatePush(
            request.RecipientId,
            request.Title,
            request.Content);

        var created = await repository.AddAsync(notification, cancellationToken);

        await CreateLogEntryAsync(created.Id, created.Channel, NotificationEventType.Created,
            "Push notification created", cancellationToken);

        return CreatedAtAction(nameof(GetNotificationById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetAllNotifications(
        CancellationToken cancellationToken)
    {
        var notifications = await repository.GetAllAsync(cancellationToken);
        return Ok(notifications.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> GetNotificationById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(id, cancellationToken);
        if (notification is null)
        {
            return NotFound(new { message = $"Notification with ID {id} not found" });
        }

        return Ok(MapToResponse(notification));
    }

    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(id, cancellationToken);
        if (notification is null)
        {
            return NotFound(new { message = $"Notification with ID {id} not found" });
        }

        if (notification.Channel != NotificationChannel.Push)
        {
            return BadRequest(new { message = "Only push notifications can be marked as read" });
        }

        try
        {
            notification.MarkAsRead();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var updated = await repository.UpdateAsync(notification, cancellationToken);
        return Ok(MapToResponse(updated));
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Channel = notification.Channel,
            Status = notification.Status,
            RecipientId = notification.RecipientId,
            CreatedAt = notification.CreatedAtUtc,
            UpdatedAt = notification.UpdatedAtUtc,
            SentAt = notification.SentAtUtc,
            ErrorMessage = notification.ErrorMessage,
            RetryCount = notification.RetryCount,
            Subject = notification.Subject,
            HtmlBody = notification.HtmlBody,
            Content = notification.SmsContent ?? notification.PushContent,
            TemplateName = notification.TemplateName,
            Parameters = notification.TemplateParameters == null
                ? null
                : new Dictionary<string, string>(notification.TemplateParameters),
            Title = notification.PushTitle,
            ReadAt = notification.ReadAtUtc
        };
    }

    private async Task CreateLogEntryAsync(
        Guid notificationId,
        NotificationChannel channel,
        NotificationEventType eventType,
        string? message,
        CancellationToken cancellationToken)
    {
        var logEntry = new NotificationLog
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            Channel = channel,
            EventType = eventType,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await logRepository.AddAsync(logEntry, cancellationToken);
    }
}
