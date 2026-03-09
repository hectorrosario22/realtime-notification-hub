using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Exceptions;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Domain.Entities;

public sealed class EmailNotification : Notification
{
    public EmailAddress To { get; private init; } = default!;
    public EmailAddress? From { get; private init; }
    public EmailAddress? ReplyTo { get; private init; }
    public List<EmailAddress> Cc { get; private init; } = [];
    public List<EmailAddress> Bcc { get; private init; } = [];
    public bool IsHtml { get; private init; }
    public List<string> AttachmentUrls { get; private init; } = [];

    // EF Core
    private EmailNotification() { }

    public EmailNotification(
        Guid recipientId,
        string subject,
        string body,
        EmailAddress to,
        NotificationPriority priority = NotificationPriority.Normal,
        EmailAddress? from = null,
        EmailAddress? replyTo = null,
        IEnumerable<EmailAddress>? cc = null,
        IEnumerable<EmailAddress>? bcc = null,
        bool isHtml = false,
        IEnumerable<string>? attachmentUrls = null,
        NotificationMetadata? metadata = null)
        : base(recipientId, subject, body, NotificationChannel.Email, priority, metadata)
    {
        To = to ?? throw new DomainException("Email recipient (To) is required.");
        From = from;
        ReplyTo = replyTo;
        Cc = cc?.ToList() ?? [];
        Bcc = bcc?.ToList() ?? [];
        IsHtml = isHtml;
        AttachmentUrls = attachmentUrls?.ToList() ?? [];
    }
}
