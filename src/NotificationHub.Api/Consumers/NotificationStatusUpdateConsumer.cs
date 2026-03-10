using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationHub.Api.Hubs;
using NotificationHub.Application.Contracts;
using NotificationHub.Application.Interfaces;

namespace NotificationHub.Api.Consumers;

public sealed class NotificationStatusUpdateConsumer(
    INotificationRepository repository,
    IHubContext<PushNotificationHub> hubContext,
    ILogger<NotificationStatusUpdateConsumer> logger) : IConsumer<NotificationStatusUpdate>
{
    public async Task Consume(ConsumeContext<NotificationStatusUpdate> context)
    {
        var msg = context.Message;

        var notification = await repository.GetByIdAsync(msg.NotificationId, context.CancellationToken);
        if (notification is null)
        {
            logger.LogWarning(
                "Notification {NotificationId} not found when processing status update",
                msg.NotificationId);
            return;
        }

        if (msg.Success)
        {
            notification.MarkAsSent();
            logger.LogInformation(
                "Notification {NotificationId} marked as Sent (ref: {ExternalRef})",
                msg.NotificationId, msg.ExternalReferenceId);
        }
        else
        {
            notification.MarkAsFailed(msg.ErrorMessage ?? "Unknown error");
            logger.LogWarning(
                "Notification {NotificationId} marked as Failed: {Error}",
                msg.NotificationId, msg.ErrorMessage);
        }

        await repository.UpdateAsync(notification, context.CancellationToken);

        await hubContext.Clients
            .Group($"recipient-{msg.RecipientId}")
            .SendAsync("NotificationStatusChanged", new
            {
                notificationId = msg.NotificationId,
                channel = msg.Channel,
                success = msg.Success,
                externalReferenceId = msg.ExternalReferenceId,
                timestamp = msg.Timestamp
            }, context.CancellationToken);
    }
}
