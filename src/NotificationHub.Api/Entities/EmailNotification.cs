using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Represents an email notification with subject and HTML body.
/// </summary>
public class EmailNotification : Notification
{
    /// <summary>
    /// Email subject line
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// Email body in HTML format
    /// </summary>
    public required string HtmlBody { get; set; }

    public EmailNotification()
    {
        Channel = NotificationChannel.Email;
    }
}
