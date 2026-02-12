using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Interfaces;

public interface INotificationLogRepository
{
    Task<NotificationLog> AddAsync(NotificationLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationLog>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
}
