using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Represents an SMS notification with text content.
/// </summary>
public class SmsNotification : Notification
{
    /// <summary>
    /// SMS message content (max 1600 characters for concatenated messages)
    /// </summary>
    public required string Content { get; set; }

    public SmsNotification()
    {
        Channel = NotificationChannel.Sms;
    }
}
