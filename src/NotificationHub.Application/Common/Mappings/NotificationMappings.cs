using NotificationHub.Application.Common.Models;
using NotificationHub.Domain.Notifications;

namespace NotificationHub.Application.Common.Mappings;

public static class NotificationMappings
{
    public static NotificationResult ToResult(this Notification notification)
    {
        return new NotificationResult(
            notification.Id,
            notification.Channel,
            notification.Status,
            notification.RecipientId,
            notification.CreatedAtUtc,
            notification.UpdatedAtUtc,
            notification.SentAtUtc,
            notification.ErrorMessage,
            notification.RetryCount,
            notification.Subject,
            notification.HtmlBody,
            notification.SmsContent ?? notification.PushContent,
            notification.TemplateName,
            notification.TemplateParameters is null
                ? null
                : new Dictionary<string, string>(notification.TemplateParameters),
            notification.PushTitle,
            notification.ReadAtUtc);
    }
}
