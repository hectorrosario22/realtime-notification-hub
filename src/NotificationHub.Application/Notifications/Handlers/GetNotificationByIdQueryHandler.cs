using Mediator;
using NotificationHub.Application.Common.Mappings;
using NotificationHub.Application.Common.Models;
using NotificationHub.Application.Interfaces;
using NotificationHub.Application.Notifications.Queries;

namespace NotificationHub.Application.Notifications.Handlers;

public sealed class GetNotificationByIdQueryHandler(INotificationRepository notificationRepository)
    : IRequestHandler<GetNotificationByIdQuery, NotificationResult?>
{
    public async ValueTask<NotificationResult?> Handle(
        GetNotificationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(query.NotificationId, cancellationToken);
        return notification?.ToResult();
    }
}
