namespace NotificationHub.Application.DTOs;

public sealed record SendEmailNotificationRequest(
    Guid RecipientId,
    string Subject,
    string Body,
    string To,
    string? From = null,
    string? ReplyTo = null,
    List<string>? Cc = null,
    List<string>? Bcc = null,
    bool IsHtml = false,
    List<string>? AttachmentUrls = null,
    string Priority = "Normal",
    Dictionary<string, string>? Metadata = null);
