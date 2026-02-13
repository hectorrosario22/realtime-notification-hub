using Mediator;
using NotificationHub.Application.Common.Models;

namespace NotificationHub.Application.Notifications.Queries;

public sealed record GetNotificationByIdQuery(Guid NotificationId) : IRequest<NotificationResult?>;
