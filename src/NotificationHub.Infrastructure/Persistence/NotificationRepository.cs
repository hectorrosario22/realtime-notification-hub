using Microsoft.EntityFrameworkCore;
using NotificationHub.Application.Interfaces;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;

namespace NotificationHub.Infrastructure.Persistence;

public sealed class NotificationRepository(NotificationDbContext dbContext) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Notifications.FindAsync([id], ct);
    }

    public async Task<IReadOnlyList<Notification>> GetFailedAsync(CancellationToken ct = default)
    {
        return await dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.Failed)
            .OrderByDescending(n => n.FailedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await dbContext.Notifications.AddAsync(notification, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        dbContext.Notifications.Update(notification);
        await dbContext.SaveChangesAsync(ct);
    }
}
