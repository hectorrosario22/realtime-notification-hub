using NotificationHub.Domain.Common;
using NotificationHub.Domain.Enums;

namespace NotificationHub.Domain.Events;

public sealed record NotificationFailedEvent(
    Guid NotificationId,
    NotificationChannel Channel,
    Guid RecipientId,
    string Reason,
    int RetryCount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
