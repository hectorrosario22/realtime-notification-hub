namespace NotificationHub.Core.Enums;

/// <summary>
/// Represents the severity or type of notification content.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Informational notification
    /// </summary>
    Info = 1,

    /// <summary>
    /// Success notification
    /// </summary>
    Success = 2,

    /// <summary>
    /// Warning notification
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error notification
    /// </summary>
    Error = 4
}
