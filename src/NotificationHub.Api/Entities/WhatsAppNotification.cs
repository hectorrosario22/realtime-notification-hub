using NotificationHub.Api.Enums;

namespace NotificationHub.Api.Entities;

/// <summary>
/// Represents a WhatsApp notification using templates.
/// </summary>
public class WhatsAppNotification : Notification
{
    /// <summary>
    /// WhatsApp template name (registered with WhatsApp Business API)
    /// </summary>
    public required string TemplateName { get; set; }

    /// <summary>
    /// Template parameters as key-value pairs
    /// </summary>
    public Dictionary<string, string>? Parameters { get; set; }

    public WhatsAppNotification()
    {
        Channel = NotificationChannel.WhatsApp;
    }
}
