using System.ComponentModel.DataAnnotations;
using NotificationHub.Api.Enums;

namespace NotificationHub.Api.DTOs.Requests;

/// <summary>
/// Request model for sending a new notification.
/// </summary>
public record SendNotificationRequest
{
    /// <summary>
    /// Notification title/subject
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public required string Title { get; init; }

    /// <summary>
    /// Main notification message content
    /// </summary>
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public required string Message { get; init; }

    /// <summary>
    /// Delivery channel (1=Email, 2=SMS, 3=WhatsApp, 4=Push)
    /// </summary>
    [Required(ErrorMessage = "Channel is required")]
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Recipient identifier (email, phone number, user ID)
    /// </summary>
    [MaxLength(256, ErrorMessage = "RecipientId cannot exceed 256 characters")]
    public string? RecipientId { get; init; }

    /// <summary>
    /// Recipient display name
    /// </summary>
    [MaxLength(200, ErrorMessage = "RecipientName cannot exceed 200 characters")]
    public string? RecipientName { get; init; }

    /// <summary>
    /// Additional metadata for channel-specific data
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}
