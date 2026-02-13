using Mediator;
using Microsoft.AspNetCore.Mvc;
using NotificationHub.Application.Common.Models;
using NotificationHub.Application.Interfaces;
using NotificationHub.Application.Notifications.Commands;
using NotificationHub.Application.Notifications.Queries;
using NotificationHub.Api.DTOs.Requests;
using NotificationHub.Api.DTOs.Responses;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(
    IMediator mediator,
    INotificationRepository repository,
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

        var command = new CreateEmailNotificationCommand(
            request.RecipientId,
            request.Subject,
            request.HtmlBody);

        var created = await mediator.Send(command, cancellationToken);

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

        var command = new CreateSmsNotificationCommand(
            request.RecipientId,
            request.Content);

        var created = await mediator.Send(command, cancellationToken);

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

        var command = new CreateWhatsAppNotificationCommand(
            request.RecipientId,
            request.TemplateName,
            request.Parameters);

        var created = await mediator.Send(command, cancellationToken);

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

        var command = new CreatePushNotificationCommand(
            request.RecipientId,
            request.Title,
            request.Content);

        var created = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetNotificationById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetAllNotifications(
        CancellationToken cancellationToken)
    {
        var notifications = await mediator.Send(new GetAllNotificationsQuery(), cancellationToken);
        return Ok(notifications.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> GetNotificationById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await mediator.Send(new GetNotificationByIdQuery(id), cancellationToken);
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

    private static NotificationResponse MapToResponse(NotificationResult notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Channel = notification.Channel,
            Status = notification.Status,
            RecipientId = notification.RecipientId,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt,
            SentAt = notification.SentAt,
            ErrorMessage = notification.ErrorMessage,
            RetryCount = notification.RetryCount,
            Subject = notification.Subject,
            HtmlBody = notification.HtmlBody,
            Content = notification.Content,
            TemplateName = notification.TemplateName,
            Parameters = notification.Parameters == null
                ? null
                : new Dictionary<string, string>(notification.Parameters),
            Title = notification.Title,
            ReadAt = notification.ReadAt
        };
    }
}
