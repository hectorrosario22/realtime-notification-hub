namespace NotificationHub.Application.Contracts;

public sealed record EmailNotificationMessage(
    Guid NotificationId,
    Guid RecipientId,
    string Subject,
    string Body,
    string To,
    string? From,
    string? ReplyTo,
    List<string>? Cc,
    List<string>? Bcc,
    bool IsHtml,
    List<string>? AttachmentUrls,
    string Priority,
    DateTime QueuedAt);
