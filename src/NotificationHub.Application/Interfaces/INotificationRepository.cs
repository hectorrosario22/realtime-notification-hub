using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByRecipientAsync(string recipientId, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
