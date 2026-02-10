using System.ComponentModel.DataAnnotations;

namespace NotificationHub.Api.DTOs.Requests;

/// <summary>
/// Request model for sending a push notification.
/// </summary>
public record SendPushRequest
{
    /// <summary>
    /// Recipient user/device identifier
    /// </summary>
    [Required(ErrorMessage = "RecipientId (user/device ID) is required")]
    [MaxLength(256, ErrorMessage = "RecipientId cannot exceed 256 characters")]
    public required string RecipientId { get; init; }

    /// <summary>
    /// Push notification title
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public required string Title { get; init; }

    /// <summary>
    /// Push notification content (supports markdown)
    /// </summary>
    [Required(ErrorMessage = "Content is required")]
    [MaxLength(2000, ErrorMessage = "Content cannot exceed 2000 characters")]
    public required string Content { get; init; }
}
