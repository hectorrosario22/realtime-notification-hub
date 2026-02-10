using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Represents an audit log entry for notification lifecycle events.
/// </summary>
public class NotificationLog
{
    /// <summary>
    /// Unique identifier for the log entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the notification this log belongs to
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// Navigation property to the notification
    /// </summary>
    public Notification? Notification { get; set; }

    /// <summary>
    /// Notification channel
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Type of event being logged
    /// </summary>
    public NotificationEventType EventType { get; set; }

    /// <summary>
    /// Human-readable description of the event
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Request data/payload sent (JSON format)
    /// </summary>
    public string? RequestData { get; set; }

    /// <summary>
    /// Response data received from provider (JSON format)
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// Error details including stack trace if applicable
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// When this event occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
}
