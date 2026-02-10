using NotificationHub.Api.Enums;

namespace NotificationHub.Api.DTOs.Responses;

/// <summary>
/// Polymorphic response model for notification data.
/// Includes all channel-specific properties (null if not applicable).
/// </summary>
public record NotificationResponse
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Delivery channel (Email, SMS, WhatsApp, Push)
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Current status in the notification lifecycle
    /// </summary>
    public NotificationStatus Status { get; init; }

    /// <summary>
    /// Recipient identifier (email, phone number, user ID)
    /// </summary>
    public string? RecipientId { get; init; }

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
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; init; }

    // Email-specific properties
    /// <summary>
    /// Email subject line (Email notifications only)
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Email body in HTML format (Email notifications only)
    /// </summary>
    public string? HtmlBody { get; init; }

    // SMS-specific properties
    /// <summary>
    /// SMS message content (SMS notifications only)
    /// </summary>
    public string? Content { get; init; }

    // WhatsApp-specific properties
    /// <summary>
    /// WhatsApp template name (WhatsApp notifications only)
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    /// Template parameters (WhatsApp notifications only)
    /// </summary>
    public Dictionary<string, string>? Parameters { get; init; }

    // Push-specific properties
    /// <summary>
    /// Push notification title (Push notifications only)
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// When the notification was read by the recipient (Push notifications only)
    /// </summary>
    public DateTime? ReadAt { get; init; }
}
