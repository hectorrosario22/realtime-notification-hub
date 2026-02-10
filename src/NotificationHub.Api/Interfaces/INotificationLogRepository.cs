using NotificationHub.Api.Entities;
using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Interfaces;

/// <summary>
/// Repository interface for notification audit logs.
/// </summary>
public interface INotificationLogRepository
{
    /// <summary>
    /// Adds a new log entry.
    /// </summary>
    /// <param name="log">The log entry to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created log entry</returns>
    Task<NotificationLog> AddAsync(NotificationLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all log entries for a specific notification.
    /// </summary>
    /// <param name="notificationId">The notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of log entries for the notification</returns>
    Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all log entries for a specific channel.
    /// </summary>
    /// <param name="channel">The notification channel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of log entries for the channel</returns>
    Task<IEnumerable<NotificationLog>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
}
