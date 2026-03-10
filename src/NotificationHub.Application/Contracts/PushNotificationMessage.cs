namespace NotificationHub.Application.Contracts;

public sealed record PushNotificationMessage(
    Guid NotificationId,
    string Title,
    string Body,
    string Priority,
    IReadOnlyDictionary<string, string> Metadata,
    DateTime SentAt);
