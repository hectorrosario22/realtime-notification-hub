namespace NotificationHub.Core.Enums;

/// <summary>
/// Represents the delivery channel for a notification.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Email notification
    /// </summary>
    Email = 1,

    /// <summary>
    /// SMS text message notification
    /// </summary>
    Sms = 2,

    /// <summary>
    /// WhatsApp message notification
    /// </summary>
    WhatsApp = 3,

    /// <summary>
    /// Push notification (mobile/web)
    /// </summary>
    Push = 4
}
