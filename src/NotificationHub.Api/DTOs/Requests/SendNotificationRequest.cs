using System.ComponentModel.DataAnnotations;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.DTOs.Requests;

public record SendNotificationRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public required string Title { get; init; }

    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public required string Message { get; init; }

    [Required(ErrorMessage = "Channel is required")]
    public NotificationChannel Channel { get; init; }

    [MaxLength(256, ErrorMessage = "RecipientId cannot exceed 256 characters")]
    public string? RecipientId { get; init; }

    [MaxLength(200, ErrorMessage = "RecipientName cannot exceed 200 characters")]
    public string? RecipientName { get; init; }

    public Dictionary<string, string>? Metadata { get; init; }
}
