using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Exceptions;
using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Domain.Entities;

public sealed class SmsNotification : Notification
{
    public PhoneNumber PhoneNumber { get; private init; } = default!;
    public PhoneNumber? FromNumber { get; private init; }

    // EF Core
    private SmsNotification() { }

    public SmsNotification(
        Guid recipientId,
        string title,
        string body,
        PhoneNumber phoneNumber,
        NotificationPriority priority = NotificationPriority.Normal,
        PhoneNumber? fromNumber = null,
        NotificationMetadata? metadata = null)
        : base(recipientId, title, body, NotificationChannel.Sms, priority, metadata)
    {
        PhoneNumber = phoneNumber ?? throw new DomainException("SMS phone number is required.");
        FromNumber = fromNumber;
    }
}
