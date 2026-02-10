using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Represents a notification entity that can be sent through various channels.
/// </summary>
public class Notification
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Notification title/subject
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Main notification message content
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Delivery channel (Email, SMS, WhatsApp, Push)
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Notification type/severity (Info, Success, Warning, Error)
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Current status in the notification lifecycle
    /// </summary>
    public NotificationStatus Status { get; set; }

    /// <summary>
    /// Recipient identifier (email, phone number, user ID)
    /// </summary>
    public string? RecipientId { get; set; }

    /// <summary>
    /// Recipient display name
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the notification was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// When the notification was successfully sent
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// When the notification was read by the recipient
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Flexible metadata for channel-specific data
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
