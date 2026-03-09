using NotificationHub.Domain.Enums;

namespace NotificationHub.Domain.Exceptions;

public sealed class InvalidNotificationStateException(
    NotificationStatus currentStatus,
    NotificationStatus targetStatus)
    : DomainException($"Cannot transition notification from '{currentStatus}' to '{targetStatus}'.")
{
}
