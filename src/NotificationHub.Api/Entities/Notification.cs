using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Base class for all notification types using Table-Per-Hierarchy (TPH) inheritance.
/// </summary>
public abstract class Notification
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Delivery channel (Email, SMS, WhatsApp, Push)
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Current status in the notification lifecycle
    /// </summary>
    public NotificationStatus Status { get; set; }

    /// <summary>
    /// Recipient identifier (email, phone number, user ID)
    /// </summary>
    public string? RecipientId { get; set; }

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
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Error message if delivery failed (last error)
    /// </summary>
    public string? ErrorMessage { get; set; }
}
