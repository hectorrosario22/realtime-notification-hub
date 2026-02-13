using Mediator;
using NotificationHub.Application.Common.Models;

namespace NotificationHub.Application.Notifications.Commands;

public sealed record CreateWhatsAppNotificationCommand(
    string RecipientId,
    string TemplateName,
    IReadOnlyDictionary<string, string>? Parameters) : IRequest<NotificationResult>;
