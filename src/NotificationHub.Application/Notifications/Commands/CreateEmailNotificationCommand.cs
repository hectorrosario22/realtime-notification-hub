using Mediator;
using NotificationHub.Application.Common.Models;

namespace NotificationHub.Application.Notifications.Commands;

public sealed record CreateEmailNotificationCommand(
    string RecipientId,
    string Subject,
    string HtmlBody) : IRequest<NotificationResult>;
