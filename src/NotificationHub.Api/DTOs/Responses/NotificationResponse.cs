using NotificationHub.Api.Enums;

namespace NotificationHub.Api.DTOs.Responses;

/// <summary>
/// Response model for notification data.
/// </summary>
public record NotificationResponse
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Notification title/subject
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Main notification message content
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Delivery channel (Email, SMS, WhatsApp, Push)
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Notification type/severity (Info, Success, Warning, Error)
    /// </summary>
    public NotificationType Type { get; init; }

    /// <summary>
    /// Current status in the notification lifecycle
    /// </summary>
    public NotificationStatus Status { get; init; }

    /// <summary>
    /// Recipient identifier (email, phone number, user ID)
    /// </summary>
    public string? RecipientId { get; init; }

    /// <summary>
    /// Recipient display name
    /// </summary>
    public string? RecipientName { get; init; }

    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the notification was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// When the notification was successfully sent
    /// </summary>
    public DateTime? SentAt { get; init; }

    /// <summary>
    /// When the notification was read by the recipient
    /// </summary>
    public DateTime? ReadAt { get; init; }

    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Additional metadata for channel-specific data
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}
