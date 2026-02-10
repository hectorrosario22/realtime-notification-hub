using Microsoft.EntityFrameworkCore;
using NotificationHub.Core.Entities;
using NotificationHub.Core.Enums;
using NotificationHub.Core.Interfaces;
using NotificationHub.Infrastructure.Data;

namespace NotificationHub.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for notification data access.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.Status == status)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetByRecipientAsync(string recipientId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        // Set default values
        if (notification.Id == Guid.Empty)
        {
            notification.Id = Guid.NewGuid();
        }

        if (notification.CreatedAt == default)
        {
            notification.CreatedAt = DateTime.UtcNow;
        }

        if (notification.Status == default)
        {
            notification.Status = NotificationStatus.Pending;
        }

        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        notification.UpdatedAt = DateTime.UtcNow;

        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return notification;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(id, cancellationToken);
        if (notification == null)
        {
            return false;
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
