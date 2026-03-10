using Microsoft.Extensions.Logging;
using NotificationHub.Application.Contracts;
using NotificationHub.Application.DTOs;
using NotificationHub.Application.Interfaces;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Application.Services;

internal sealed class NotificationService(
    INotificationRepository repository,
    IPushNotificationSender pushSender,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<NotificationResponse> SendPushAsync(
        SendPushNotificationRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<NotificationPriority>(request.Priority, ignoreCase: true, out var priority))
            priority = NotificationPriority.Normal;

        var metadata = request.Metadata is not null
            ? NotificationMetadata.Create(request.Metadata)
            : null;

        var notification = new PushNotification(
            request.RecipientId,
            request.Title,
            request.Body,
            priority,
            metadata);

        await repository.AddAsync(notification, ct);

        logger.LogInformation(
            "Push notification {NotificationId} created for recipient {RecipientId}",
            notification.Id, notification.RecipientId);

        var sentAt = DateTime.UtcNow;
        var message = new PushNotificationMessage(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.Priority.ToString(),
            notification.Metadata.Data,
            sentAt);

        try
        {
            await pushSender.SendToRecipientAsync(notification.RecipientId, message, ct);
            notification.MarkAsSent();
            await repository.UpdateAsync(notification, ct);

            logger.LogInformation(
                "Push notification {NotificationId} sent to recipient {RecipientId}",
                notification.Id, notification.RecipientId);
        }
        catch (Exception ex)
        {
            notification.MarkAsFailed(ex.Message);
            await repository.UpdateAsync(notification, ct);

            logger.LogError(ex,
                "Failed to send push notification {NotificationId} to recipient {RecipientId}",
                notification.Id, notification.RecipientId);
        }

        return MapToResponse(notification);
    }

    public async Task<NotificationResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await repository.GetByIdAsync(id, ct);
        return notification is not null ? MapToResponse(notification) : null;
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetFailedAsync(CancellationToken ct = default)
    {
        var notifications = await repository.GetFailedAsync(ct);
        return notifications.Select(MapToResponse).ToList();
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.RecipientId,
            notification.Title,
            notification.Body,
            notification.Channel.ToString(),
            notification.Status.ToString(),
            notification.Priority.ToString(),
            notification.CreatedAt,
            notification.SentAt,
            notification.FailedAt,
            notification.ErrorMessage);
    }
}
