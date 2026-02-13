using NotificationHub.Domain.Abstractions;
using NotificationHub.Domain.Notifications.Events;

namespace NotificationHub.Domain.Notifications;

public sealed class Notification : AggregateRoot<Guid>
{
    private static readonly IReadOnlyDictionary<NotificationStatus, HashSet<NotificationStatus>> AllowedTransitions =
        new Dictionary<NotificationStatus, HashSet<NotificationStatus>>
        {
            [NotificationStatus.Pending] = [NotificationStatus.Queued],
            [NotificationStatus.Queued] = [NotificationStatus.Processing, NotificationStatus.Sent, NotificationStatus.Failed],
            [NotificationStatus.Processing] = [NotificationStatus.Sent, NotificationStatus.Failed],
            [NotificationStatus.Sent] = [NotificationStatus.Read],
            [NotificationStatus.Failed] = [NotificationStatus.Queued],
            [NotificationStatus.Read] = []
        };

    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string RecipientId { get; private set; } = string.Empty;

    public string? Subject { get; private set; }
    public string? HtmlBody { get; private set; }
    public string? SmsContent { get; private set; }
    public string? TemplateName { get; private set; }
    public IReadOnlyDictionary<string, string>? TemplateParameters { get; private set; }
    public string? PushTitle { get; private set; }
    public string? PushContent { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? ScheduledAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }

    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Notification()
    {
    }

    public static Notification CreateEmail(
        string recipientId,
        string subject,
        string htmlBody,
        NotificationPriority priority = NotificationPriority.Normal,
        int maxRetries = 3)
    {
        var notification = CreateBase(NotificationChannel.Email, recipientId, priority, maxRetries);
        notification.Subject = RequireNonEmpty(subject, nameof(subject));
        notification.HtmlBody = RequireNonEmpty(htmlBody, nameof(htmlBody));
        return notification;
    }

    public static Notification CreateSms(
        string recipientId,
        string content,
        NotificationPriority priority = NotificationPriority.Normal,
        int maxRetries = 3)
    {
        var notification = CreateBase(NotificationChannel.Sms, recipientId, priority, maxRetries);
        notification.SmsContent = RequireNonEmpty(content, nameof(content));
        return notification;
    }

    public static Notification CreateWhatsApp(
        string recipientId,
        string templateName,
        IReadOnlyDictionary<string, string>? templateParameters,
        NotificationPriority priority = NotificationPriority.Normal,
        int maxRetries = 3)
    {
        var notification = CreateBase(NotificationChannel.WhatsApp, recipientId, priority, maxRetries);
        notification.TemplateName = RequireNonEmpty(templateName, nameof(templateName));
        notification.TemplateParameters = templateParameters is null
            ? null
            : new Dictionary<string, string>(templateParameters);
        return notification;
    }

    public static Notification CreatePush(
        string recipientId,
        string title,
        string content,
        NotificationPriority priority = NotificationPriority.Normal,
        int maxRetries = 3)
    {
        var notification = CreateBase(NotificationChannel.Push, recipientId, priority, maxRetries);
        notification.PushTitle = RequireNonEmpty(title, nameof(title));
        notification.PushContent = RequireNonEmpty(content, nameof(content));
        return notification;
    }

    public void ScheduleFor(DateTime scheduledAtUtc)
    {
        if (scheduledAtUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Scheduled time must be in the future.");
        }

        ScheduledAtUtc = scheduledAtUtc;
        Touch();
    }

    public void MarkAsQueued()
    {
        EnsureTransitionTo(NotificationStatus.Queued);

        if (Status == NotificationStatus.Failed && !CanRetry())
        {
            throw new InvalidOperationException("Cannot queue notification: maximum retries reached.");
        }

        ChangeStatus(NotificationStatus.Queued);
        ErrorMessage = null;
    }

    public void StartProcessing()
    {
        EnsureTransitionTo(NotificationStatus.Processing);
        ChangeStatus(NotificationStatus.Processing);
    }

    public void MarkAsSent(DateTime? sentAtUtc = null)
    {
        EnsureTransitionTo(NotificationStatus.Sent);
        SentAtUtc = sentAtUtc ?? DateTime.UtcNow;
        ErrorMessage = null;
        ChangeStatus(NotificationStatus.Sent);
    }

    public void MarkAsFailed(string errorMessage)
    {
        EnsureTransitionTo(NotificationStatus.Failed);
        ErrorMessage = RequireNonEmpty(errorMessage, nameof(errorMessage));
        RetryCount++;
        ChangeStatus(NotificationStatus.Failed);
    }

    public void MarkAsRead(DateTime? readAtUtc = null)
    {
        EnsureTransitionTo(NotificationStatus.Read);
        ReadAtUtc = readAtUtc ?? DateTime.UtcNow;
        ChangeStatus(NotificationStatus.Read);
    }

    public bool CanRetry()
    {
        return Status == NotificationStatus.Failed && RetryCount < MaxRetries;
    }

    private static Notification CreateBase(
        NotificationChannel channel,
        string recipientId,
        NotificationPriority priority,
        int maxRetries)
    {
        if (maxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries cannot be negative.");
        }

        var now = DateTime.UtcNow;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            Status = NotificationStatus.Pending,
            Priority = priority,
            RecipientId = RequireNonEmpty(recipientId, nameof(recipientId)),
            CreatedAtUtc = now,
            MaxRetries = maxRetries,
            RetryCount = 0
        };

        notification.RaiseDomainEvent(
            new NotificationCreatedDomainEvent(notification.Id, notification.Channel, notification.RecipientId));

        return notification;
    }

    private void EnsureTransitionTo(NotificationStatus targetStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var nextStatuses) || !nextStatuses.Contains(targetStatus))
        {
            throw new InvalidOperationException(
                $"Invalid state transition: {Status} -> {targetStatus}.");
        }
    }

    private void ChangeStatus(NotificationStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        var previous = Status;
        Status = newStatus;
        Touch();
        RaiseDomainEvent(new NotificationStatusChangedDomainEvent(Id, previous, newStatus));
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string RequireNonEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        }

        return value.Trim();
    }
}
