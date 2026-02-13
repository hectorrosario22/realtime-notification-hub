using Mediator;
using NotificationHub.Application.Common.Mappings;
using NotificationHub.Application.Common.Models;
using NotificationHub.Application.Interfaces;
using NotificationHub.Application.Notifications.Commands;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Notifications.Handlers;

public sealed class CreatePushNotificationCommandHandler(
    INotificationRepository notificationRepository,
    INotificationLogRepository logRepository)
    : IRequestHandler<CreatePushNotificationCommand, NotificationResult>
{
    public async ValueTask<NotificationResult> Handle(
        CreatePushNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var notification = Notification.CreatePush(
            command.RecipientId,
            command.Title,
            command.Content);

        var created = await notificationRepository.AddAsync(notification, cancellationToken);

        var logEntry = new NotificationLog
        {
            Id = Guid.NewGuid(),
            NotificationId = created.Id,
            Channel = created.Channel,
            EventType = NotificationEventType.Created,
            Message = "Push notification created",
            Timestamp = DateTime.UtcNow
        };

        await logRepository.AddAsync(logEntry, cancellationToken);

        return created.ToResult();
    }
}
