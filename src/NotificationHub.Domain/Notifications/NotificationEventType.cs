namespace NotificationHub.Domain.Notifications;

public enum NotificationEventType
{
    Created = 1,
    Queued = 2,
    ProcessingStarted = 3,
    Delivered = 4,
    Failed = 5,
    Read = 6,
    Retried = 7
}
