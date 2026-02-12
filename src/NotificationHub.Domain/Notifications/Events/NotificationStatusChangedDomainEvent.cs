using NotificationHub.Domain.Abstractions;

namespace NotificationHub.Domain.Notifications.Events;

public sealed record NotificationStatusChangedDomainEvent(
    Guid NotificationId,
    NotificationStatus PreviousStatus,
    NotificationStatus CurrentStatus) : DomainEvent;
