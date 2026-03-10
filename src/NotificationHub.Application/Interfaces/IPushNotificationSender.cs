using NotificationHub.Application.Contracts;

namespace NotificationHub.Application.Interfaces;

public interface IPushNotificationSender
{
    Task SendToRecipientAsync(Guid recipientId, PushNotificationMessage message, CancellationToken ct = default);
}
