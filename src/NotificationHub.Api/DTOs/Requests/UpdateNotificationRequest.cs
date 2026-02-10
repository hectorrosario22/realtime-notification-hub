using NotificationHub.Api.Enums;

namespace NotificationHub.Api.DTOs.Requests;

/// <summary>
/// Request model for updating a notification.
/// </summary>
public record UpdateNotificationRequest
{
    /// <summary>
    /// Updated notification status
    /// </summary>
    public NotificationStatus? Status { get; init; }

    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}
