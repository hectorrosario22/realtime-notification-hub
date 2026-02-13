using Mediator;
using NotificationHub.Application.Common.Models;

namespace NotificationHub.Application.Notifications.Commands;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<MarkNotificationAsReadResult>;
