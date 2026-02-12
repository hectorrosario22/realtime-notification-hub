namespace NotificationHub.Domain.Notifications;

public enum NotificationStatus
{
    Pending = 1,
    Queued = 2,
    Processing = 3,
    Sent = 4,
    Failed = 5,
    Read = 6
}
