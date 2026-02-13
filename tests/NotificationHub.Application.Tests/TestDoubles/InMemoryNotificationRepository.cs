using NotificationHub.Application.Interfaces;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Tests.TestDoubles;

internal sealed class InMemoryNotificationRepository : INotificationRepository
{
    private readonly List<Notification> notifications = [];

    public InMemoryNotificationRepository(IEnumerable<Notification>? seed = null)
    {
        if (seed is not null)
        {
            notifications.AddRange(seed);
        }
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(notifications.FirstOrDefault(n => n.Id == id));
    }

    public Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Notification>>(notifications.ToArray());
    }

    public Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Notification>>(notifications.Where(n => n.Status == status).ToArray());
    }

    public Task<IEnumerable<Notification>> GetByRecipientAsync(string recipientId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Notification>>(notifications.Where(n => n.RecipientId == recipientId).ToArray());
    }

    public Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        notifications.Add(notification);
        return Task.FromResult(notification);
    }

    public Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        var index = notifications.FindIndex(n => n.Id == notification.Id);
        if (index >= 0)
        {
            notifications[index] = notification;
        }

        return Task.FromResult(notification);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = notifications.RemoveAll(n => n.Id == id) > 0;
        return Task.FromResult(removed);
    }
}
