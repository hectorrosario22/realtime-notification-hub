using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Represents a push notification with title, content, and read tracking.
/// </summary>
public class PushNotification : Notification
{
    /// <summary>
    /// Push notification title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Push notification content (supports markdown)
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// When the notification was read by the recipient
    /// </summary>
    public DateTime? ReadAt { get; set; }

    public PushNotification()
    {
        Channel = NotificationChannel.Push;
    }
}
