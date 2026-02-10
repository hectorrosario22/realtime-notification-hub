using System.ComponentModel.DataAnnotations;

namespace NotificationHub.Api.DTOs.Requests;

/// <summary>
/// Request model for sending a WhatsApp notification.
/// </summary>
public record SendWhatsAppRequest
{
    /// <summary>
    /// Recipient phone number (E.164 format recommended)
    /// </summary>
    [Required(ErrorMessage = "RecipientId (phone number) is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(256, ErrorMessage = "Phone number cannot exceed 256 characters")]
    public required string RecipientId { get; init; }

    /// <summary>
    /// WhatsApp template name (must be registered with WhatsApp Business API)
    /// </summary>
    [Required(ErrorMessage = "TemplateName is required")]
    [MaxLength(100, ErrorMessage = "TemplateName cannot exceed 100 characters")]
    public required string TemplateName { get; init; }

    /// <summary>
    /// Template parameters as key-value pairs
    /// </summary>
    public Dictionary<string, string>? Parameters { get; init; }
}
