using Mediator;
using NotificationHub.Application.Common.Mappings;
using NotificationHub.Application.Common.Models;
using NotificationHub.Application.Interfaces;
using NotificationHub.Application.Notifications.Commands;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Notifications.Handlers;

public sealed class MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository)
    : IRequestHandler<MarkNotificationAsReadCommand, MarkNotificationAsReadResult>
{
    public async ValueTask<MarkNotificationAsReadResult> Handle(
        MarkNotificationAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(command.NotificationId, cancellationToken);
        if (notification is null)
        {
            return new MarkNotificationAsReadResult(
                null,
                MarkNotificationAsReadError.NotFound,
                $"Notification with ID {command.NotificationId} not found");
        }

        if (notification.Channel != NotificationChannel.Push)
        {
            return new MarkNotificationAsReadResult(
                null,
                MarkNotificationAsReadError.InvalidChannel,
                "Only push notifications can be marked as read");
        }

        try
        {
            notification.MarkAsRead();
        }
        catch (InvalidOperationException ex)
        {
            return new MarkNotificationAsReadResult(
                null,
                MarkNotificationAsReadError.InvalidStateTransition,
                ex.Message);
        }

        var updated = await notificationRepository.UpdateAsync(notification, cancellationToken);
        return new MarkNotificationAsReadResult(
            updated.ToResult(),
            MarkNotificationAsReadError.None,
            null);
    }
}
