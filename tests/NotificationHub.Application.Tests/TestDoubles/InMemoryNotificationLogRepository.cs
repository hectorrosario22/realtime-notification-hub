using NotificationHub.Application.Interfaces;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Tests.TestDoubles;

internal sealed class InMemoryNotificationLogRepository : INotificationLogRepository
{
    private readonly List<NotificationLog> logs = [];

    public Task<NotificationLog> AddAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        logs.Add(log);
        return Task.FromResult(log);
    }

    public Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var filtered = logs.Where(l => l.NotificationId == notificationId).ToArray();
        return Task.FromResult<IEnumerable<NotificationLog>>(filtered);
    }

    public Task<IEnumerable<NotificationLog>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        var filtered = logs.Where(l => l.Channel == channel).ToArray();
        return Task.FromResult<IEnumerable<NotificationLog>>(filtered);
    }

    public IReadOnlyCollection<NotificationLog> Logs => logs;
}
