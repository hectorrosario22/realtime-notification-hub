using NotificationHub.Application.DTOs;

namespace NotificationHub.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationResponse> SendPushAsync(SendPushNotificationRequest request, CancellationToken ct = default);
    Task<NotificationResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationResponse>> GetFailedAsync(CancellationToken ct = default);
}
