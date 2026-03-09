using NotificationHub.Domain.Enums;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Domain.Entities;

public sealed class PushNotification : Notification
{
    // EF Core
    private PushNotification() { }

    public PushNotification(
        Guid recipientId,
        string title,
        string body,
        NotificationPriority priority = NotificationPriority.Normal,
        NotificationMetadata? metadata = null)
        : base(recipientId, title, body, NotificationChannel.Push, priority, metadata)
    {
    }
}
