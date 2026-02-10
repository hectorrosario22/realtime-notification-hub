using Microsoft.EntityFrameworkCore;
using NotificationHub.Api.Data;
using NotificationHub.Api.Entities;
using NotificationHub.Api.Enums;
using NotificationHub.Api.Interfaces;

namespace NotificationHub.Api.Repositories;

/// <summary>
/// Repository implementation for notification audit logs.
/// </summary>
public class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _context;

    public NotificationLogRepository(NotificationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<NotificationLog> AddAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
        return log;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationLogs
            .Where(l => l.NotificationId == notificationId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<NotificationLog>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationLogs
            .Where(l => l.Channel == channel)
            .OrderByDescending(l => l.Timestamp)
            .Take(100) // Limit results for performance
            .ToListAsync(cancellationToken);
    }
}
