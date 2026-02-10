namespace NotificationHub.Api.Enums;

/// <summary>
/// Represents the type of event in a notification's lifecycle.
/// </summary>
public enum NotificationEventType
{
    /// <summary>
    /// Notification was created
    /// </summary>
    Created = 1,

    /// <summary>
    /// Notification was queued in message broker
    /// </summary>
    Queued = 2,

    /// <summary>
    /// Notification is being processed by a worker
    /// </summary>
    Processing = 3,

    /// <summary>
    /// Notification was successfully sent
    /// </summary>
    Sent = 4,

    /// <summary>
    /// Notification delivery failed
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Notification is being retried
    /// </summary>
    Retry = 6,

    /// <summary>
    /// Notification was cancelled
    /// </summary>
    Cancelled = 7
}
