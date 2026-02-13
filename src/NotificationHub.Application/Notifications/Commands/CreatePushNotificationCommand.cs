using Mediator;
using NotificationHub.Application.Common.Models;

namespace NotificationHub.Application.Notifications.Commands;

public sealed record CreatePushNotificationCommand(
    string RecipientId,
    string Title,
    string Content) : IRequest<NotificationResult>;
