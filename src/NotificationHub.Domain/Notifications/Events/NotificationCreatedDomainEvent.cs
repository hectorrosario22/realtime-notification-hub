using NotificationHub.Domain.Abstractions;

namespace NotificationHub.Domain.Notifications.Events;

public sealed record NotificationCreatedDomainEvent(
    Guid NotificationId,
    NotificationChannel Channel,
    string RecipientId) : DomainEvent;
