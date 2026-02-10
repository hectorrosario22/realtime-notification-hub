using System.ComponentModel.DataAnnotations;

namespace NotificationHub.Api.DTOs.Requests;

/// <summary>
/// Request model for sending an SMS notification.
/// </summary>
public record SendSmsRequest
{
    /// <summary>
    /// Recipient phone number (E.164 format recommended)
    /// </summary>
    [Required(ErrorMessage = "RecipientId (phone number) is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(256, ErrorMessage = "Phone number cannot exceed 256 characters")]
    public required string RecipientId { get; init; }

    /// <summary>
    /// SMS message content (max 1600 characters for concatenated messages)
    /// </summary>
    [Required(ErrorMessage = "Content is required")]
    [MaxLength(1600, ErrorMessage = "Content cannot exceed 1600 characters")]
    public required string Content { get; init; }
}
