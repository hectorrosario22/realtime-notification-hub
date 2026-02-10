using Microsoft.AspNetCore.Mvc;
using NotificationHub.Api.DTOs.Requests;
using NotificationHub.Api.DTOs.Responses;
using NotificationHub.Core.Entities;
using NotificationHub.Core.Enums;
using NotificationHub.Core.Interfaces;

namespace NotificationHub.Api.Controllers;

/// <summary>
/// Manages notification operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController(
    INotificationRepository repository,
    ILogger<NotificationsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Sends a new notification.
    /// </summary>
    /// <param name="request">Notification details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created notification</returns>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> SendNotification(
        [FromBody] SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending notification: {Title} via {Channel}", request.Title, request.Channel);

        var notification = new Notification
        {
            Title = request.Title,
            Message = request.Message,
            Channel = request.Channel,
            Type = request.Type,
            RecipientId = request.RecipientId,
            RecipientName = request.RecipientName,
            Metadata = request.Metadata
        };

        var created = await repository.AddAsync(notification, cancellationToken);

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
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated notification</returns>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        notification.Status = NotificationStatus.Read;
        notification.ReadAt = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(notification, cancellationToken);

        return Ok(MapToResponse(updated));
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Channel = notification.Channel,
            Type = notification.Type,
            Status = notification.Status,
            RecipientId = notification.RecipientId,
            RecipientName = notification.RecipientName,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt,
            SentAt = notification.SentAt,
            ReadAt = notification.ReadAt,
            ErrorMessage = notification.ErrorMessage,
            RetryCount = notification.RetryCount,
            Metadata = notification.Metadata
        };
    }
}
