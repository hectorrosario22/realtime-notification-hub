using NotificationHub.Domain.Common;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Events;
using NotificationHub.Domain.Exceptions;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Domain.Entities;

public abstract class Notification : AggregateRoot<Guid>
{
    public const int MaxRetryCount = 3;

    public Guid RecipientId { get; private init; }
    public string Title { get; private init; } = default!;
    public string Body { get; private init; } = default!;
    public NotificationChannel Channel { get; private init; }
    public NotificationStatus Status { get; private set; }
    public NotificationPriority Priority { get; private init; }
    public NotificationMetadata Metadata { get; private init; } = NotificationMetadata.Empty();

    public DateTime CreatedAt { get; private init; }
    public DateTime? QueuedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? FailedAt { get; private set; }

    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    // EF Core
    protected Notification() { }

    protected Notification(
        Guid recipientId,
        string title,
        string body,
        NotificationChannel channel,
        NotificationPriority priority,
        NotificationMetadata? metadata)
    {
        if (recipientId == Guid.Empty)
            throw new DomainException("RecipientId cannot be empty.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Notification title cannot be empty.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Notification body cannot be empty.");

        Id = Guid.CreateVersion7();
        RecipientId = recipientId;
        Title = title;
        Body = body;
        Channel = channel;
        Priority = priority;
        Metadata = metadata ?? NotificationMetadata.Empty();
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;

        RaiseDomainEvent(new NotificationCreatedEvent(Id, Channel, RecipientId));
    }

    public void MarkAsQueued()
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidNotificationStateException(Status, NotificationStatus.Queued);

        Status = NotificationStatus.Queued;
        QueuedAt = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        if (Status is not (NotificationStatus.Queued or NotificationStatus.Pending))
            throw new InvalidNotificationStateException(Status, NotificationStatus.Sent);

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationSentEvent(Id, Channel, RecipientId));
    }

    public void MarkAsDelivered()
    {
        if (Status != NotificationStatus.Sent)
            throw new InvalidNotificationStateException(Status, NotificationStatus.Delivered);

        Status = NotificationStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationDeliveredEvent(Id, Channel, RecipientId));
    }

    public void MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Failure reason is required.");

        Status = NotificationStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = reason;

        RaiseDomainEvent(new NotificationFailedEvent(Id, Channel, RecipientId, reason, RetryCount));
    }

    public void IncrementRetryCount()
    {
        if (RetryCount >= MaxRetryCount)
            throw new DomainException($"Retry count cannot exceed {MaxRetryCount}.");

        RetryCount++;
        Status = NotificationStatus.Queued;
        ErrorMessage = null;
        FailedAt = null;
    }

    public bool CanRetry() => RetryCount < MaxRetryCount;

    public void Cancel()
    {
        if (Status is NotificationStatus.Sent or NotificationStatus.Delivered)
            throw new InvalidNotificationStateException(Status, NotificationStatus.Cancelled);

        Status = NotificationStatus.Cancelled;
    }
}
