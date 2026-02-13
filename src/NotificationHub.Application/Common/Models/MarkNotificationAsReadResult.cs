namespace NotificationHub.Application.Common.Models;

public enum MarkNotificationAsReadError
{
    None = 0,
    NotFound,
    InvalidChannel,
    InvalidStateTransition
}

public sealed record MarkNotificationAsReadResult(
    NotificationResult? Notification,
    MarkNotificationAsReadError Error,
    string? Message)
{
    public bool IsSuccess => Error == MarkNotificationAsReadError.None && Notification is not null;
}
