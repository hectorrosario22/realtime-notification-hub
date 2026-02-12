using NotificationHub.Api.Enums;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Represents an audit log entry for notification lifecycle events.
/// </summary>
public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Notification? Notification { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationEventType EventType { get; set; }
    public string? Message { get; set; }
    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime Timestamp { get; set; }
}
