using Microsoft.AspNetCore.Mvc;
using NotificationHub.Api.DTOs.Requests;
using NotificationHub.Api.DTOs.Responses;
using NotificationHub.Api.Entities;
using NotificationHub.Api.Enums;
using NotificationHub.Api.Interfaces;

namespace NotificationHub.Api.Controllers;

/// <summary>
/// Manages notification operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController(
    INotificationRepository repository,
    INotificationLogRepository logRepository,
    ILogger<NotificationsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Sends an email notification.
    /// </summary>
    /// <param name="request">Email notification details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created notification</returns>
    [HttpPost("email")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendEmailNotification(
        [FromBody] SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending email notification to {RecipientId}", request.RecipientId);

        var notification = new EmailNotification
        {
            Id = Guid.NewGuid(),
            RecipientId = request.RecipientId,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await repository.AddAsync(notification, cancellationToken);

        // Create audit log
        await CreateLogEntryAsync(created.Id, NotificationChannel.Email, NotificationEventType.Created,
            "Email notification created", cancellationToken);

        var response = MapToResponse(created);

        return CreatedAtAction(
            nameof(GetNotificationById),
            new { id = created.Id },
            response);
    }

    /// <summary>
    /// Sends an SMS notification.
    /// </summary>
    /// <param name="request">SMS notification details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created notification</returns>
    [HttpPost("sms")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendSmsNotification(
        [FromBody] SendSmsRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending SMS notification to {RecipientId}", request.RecipientId);

        var notification = new SmsNotification
        {
            Id = Guid.NewGuid(),
            RecipientId = request.RecipientId,
            Content = request.Content,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await repository.AddAsync(notification, cancellationToken);

        // Create audit log
        await CreateLogEntryAsync(created.Id, NotificationChannel.Sms, NotificationEventType.Created,
            "SMS notification created", cancellationToken);

        var response = MapToResponse(created);

        return CreatedAtAction(
            nameof(GetNotificationById),
            new { id = created.Id },
            response);
    }

    /// <summary>
    /// Sends a WhatsApp notification.
    /// </summary>
    /// <param name="request">WhatsApp notification details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created notification</returns>
    [HttpPost("whatsapp")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendWhatsAppNotification(
        [FromBody] SendWhatsAppRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending WhatsApp notification to {RecipientId}", request.RecipientId);

        var notification = new WhatsAppNotification
        {
            Id = Guid.NewGuid(),
            RecipientId = request.RecipientId,
            TemplateName = request.TemplateName,
            Parameters = request.Parameters,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await repository.AddAsync(notification, cancellationToken);

        // Create audit log
        await CreateLogEntryAsync(created.Id, NotificationChannel.WhatsApp, NotificationEventType.Created,
            "WhatsApp notification created", cancellationToken);

        var response = MapToResponse(created);

        return CreatedAtAction(
            nameof(GetNotificationById),
            new { id = created.Id },
            response);
    }

    /// <summary>
    /// Sends a push notification.
    /// </summary>
    /// <param name="request">Push notification details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created notification</returns>
    [HttpPost("push")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendPushNotification(
        [FromBody] SendPushRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending push notification to {RecipientId}", request.RecipientId);

        var notification = new PushNotification
        {
            Id = Guid.NewGuid(),
            RecipientId = request.RecipientId,
            Title = request.Title,
            Content = request.Content,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await repository.AddAsync(notification, cancellationToken);

        // Create audit log
        await CreateLogEntryAsync(created.Id, NotificationChannel.Push, NotificationEventType.Created,
            "Push notification created", cancellationToken);

        var response = MapToResponse(created);

        return CreatedAtAction(
            nameof(GetNotificationById),
            new { id = created.Id },
            response);
    }

    /// <summary>
    /// Retrieves all notifications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all notifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetAllNotifications(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all notifications");

        var notifications = await repository.GetAllAsync(cancellationToken);
        var responses = notifications.Select(MapToResponse);

        return Ok(responses);
    }

    /// <summary>
    /// Retrieves a notification by ID.
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> GetNotificationById(
        Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving notification: {Id}", id);

        var notification = await repository.GetByIdAsync(id, cancellationToken);

        if (notification is null)
        {
            logger.LogWarning("Notification not found: {Id}", id);
            return NotFound(new { message = $"Notification with ID {id} not found" });
        }

        return Ok(MapToResponse(notification));
    }

    /// <summary>
    /// Marks a push notification as read.
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated notification</returns>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Marking notification as read: {Id}", id);

        var notification = await repository.GetByIdAsync(id, cancellationToken);

        if (notification is null)
        {
            logger.LogWarning("Notification not found: {Id}", id);
            return NotFound(new { message = $"Notification with ID {id} not found" });
        }

        if (notification is not PushNotification pushNotification)
        {
            logger.LogWarning("Cannot mark non-push notification as read: {Id}, Channel: {Channel}", id, notification.Channel);
            return BadRequest(new { message = "Only push notifications can be marked as read" });
        }

        pushNotification.Status = NotificationStatus.Read;
        pushNotification.ReadAt = DateTime.UtcNow;
        pushNotification.UpdatedAt = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(pushNotification, cancellationToken);

        return Ok(MapToResponse(updated));
    }

    /// <summary>
    /// Maps a notification entity to a polymorphic response DTO.
    /// </summary>
    private static NotificationResponse MapToResponse(Notification notification)
    {
        var response = new NotificationResponse
        {
            Id = notification.Id,
            Channel = notification.Channel,
            Status = notification.Status,
            RecipientId = notification.RecipientId,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt,
            SentAt = notification.SentAt,
            ErrorMessage = notification.ErrorMessage,
            RetryCount = notification.RetryCount
        };

        // Populate channel-specific properties based on notification type
        return notification switch
        {
            EmailNotification email => response with
            {
                Subject = email.Subject,
                HtmlBody = email.HtmlBody
            },
            SmsNotification sms => response with
            {
                Content = sms.Content
            },
            WhatsAppNotification whatsApp => response with
            {
                TemplateName = whatsApp.TemplateName,
                Parameters = whatsApp.Parameters
            },
            PushNotification push => response with
            {
                Title = push.Title,
                Content = push.Content,
                ReadAt = push.ReadAt
            },
            _ => response
        };
    }

    /// <summary>
    /// Creates an audit log entry for a notification event.
    /// </summary>
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
