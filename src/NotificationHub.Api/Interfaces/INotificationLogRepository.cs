using NotificationHub.Api.Entities;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.Interfaces;

/// <summary>
/// Repository interface for notification audit logs.
/// </summary>
public interface INotificationLogRepository
{
    Task<NotificationLog> AddAsync(NotificationLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationLog>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
}
