using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.DTOs.Requests;

public record UpdateNotificationRequest
{
    public NotificationStatus? Status { get; init; }
    public string? ErrorMessage { get; init; }
}
