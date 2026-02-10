namespace NotificationHub.Api.Enums;

/// <summary>
/// Represents the current status of a notification in its lifecycle.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification created but not yet queued
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Notification queued in message broker
    /// </summary>
    Queued = 2,

    /// <summary>
    /// Worker processing the notification
    /// </summary>
    Processing = 3,

    /// <summary>
    /// Successfully sent to recipient
    /// </summary>
    Sent = 4,

    /// <summary>
    /// Delivery failed
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Notification read by recipient
    /// </summary>
    Read = 6
}
