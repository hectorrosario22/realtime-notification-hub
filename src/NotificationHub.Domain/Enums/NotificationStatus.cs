namespace NotificationHub.Domain.Enums;

public enum NotificationStatus
{
    Pending = 0,
    Queued = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5
}
