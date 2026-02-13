using Mediator;
using NotificationHub.Application.Common.Models;

namespace NotificationHub.Application.Notifications.Commands;

public sealed record CreateSmsNotificationCommand(
    string RecipientId,
    string Content) : IRequest<NotificationResult>;
