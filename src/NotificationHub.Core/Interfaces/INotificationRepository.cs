using NotificationHub.Core.Entities;
using NotificationHub.Core.Enums;

namespace NotificationHub.Core.Interfaces;

/// <summary>
/// Repository contract for notification data access operations.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Retrieves a notification by its unique identifier.
    /// </summary>
    /// <param name="id">The notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notification if found, null otherwise</returns>
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notifications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all notifications</returns>
    Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves notifications by status.
    /// </summary>
    /// <param name="status">The notification status to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of notifications with the specified status</returns>
    Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves notifications by recipient.
    /// </summary>
    /// <param name="recipientId">The recipient identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of notifications for the specified recipient</returns>
    Task<IEnumerable<Notification>> GetByRecipientAsync(string recipientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new notification.
    /// </summary>
    /// <param name="notification">The notification to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added notification</returns>
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing notification.
    /// </summary>
    /// <param name="notification">The notification to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated notification</returns>
    Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">The notification ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
