using Microsoft.EntityFrameworkCore;
using NotificationHub.Application.Interfaces;
using NotificationHub.Api.Data;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.Repositories;

/// <summary>
/// Repository implementation for notification audit logs.
/// </summary>
public class NotificationLogRepository(NotificationDbContext context) : INotificationLogRepository
{
    public async Task<NotificationLog> AddAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        context.NotificationLogs.Add(log);
        await context.SaveChangesAsync(cancellationToken);
        return log;
    }

    public async Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await context.NotificationLogs
            .Where(l => l.NotificationId == notificationId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NotificationLog>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        return await context.NotificationLogs
            .Where(l => l.Channel == channel)
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
}
