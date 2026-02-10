using System.ComponentModel.DataAnnotations;

namespace NotificationHub.Api.DTOs.Requests;

/// <summary>
/// Request model for sending an email notification.
/// </summary>
public record SendEmailRequest
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    [Required(ErrorMessage = "RecipientId (email address) is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(256, ErrorMessage = "Email address cannot exceed 256 characters")]
    public required string RecipientId { get; init; }

    /// <summary>
    /// Email subject line
    /// </summary>
    [Required(ErrorMessage = "Subject is required")]
    [MaxLength(500, ErrorMessage = "Subject cannot exceed 500 characters")]
    public required string Subject { get; init; }

    /// <summary>
    /// Email body in HTML format
    /// </summary>
    [Required(ErrorMessage = "HtmlBody is required")]
    [MaxLength(10000, ErrorMessage = "HtmlBody cannot exceed 10000 characters")]
    public required string HtmlBody { get; init; }
}
